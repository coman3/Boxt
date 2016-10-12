app.service("BoxtService", function ($rootScope, $http, $location) {
    var scope = this;
    var facebookAuth = null;

    this.baseUrl = "http://192.168.0.17:1234/";
    this.accessData = null;
    this.loggedIn = false;
    this.userInfo = null;
    this.hub = null;
    this.hubConnected = false;
    this.navDisabled = false;
    this.backPressAction = function (event) { event.preventDefault = true; }
    this.hub = $.connection.lobbyHub;
    this.hub.client.serverMessage = function (message) {
        console.log(message);
        $rootScope.$broadcast("hub_onServerMessage", message);
    };
    document.addEventListener("deviceready", function () {
        scope.hideStatusBar();
    }, false)


    if (localStorage.getObject("appAuth") != null) {
        this.accessData = localStorage.getObject("appAuth");
        $http.defaults.headers.common.Authorization = "Bearer " + this.accessData.access_token;
        this.loggedIn = true;
    }

    //Navigate
    this.navigate = function (location) {
        $location.path("/" + location);
    }
    //Navigation Bar
    this.hideNavigation = function () {
        $rootScope.$broadcast("NavigationBar", {
            set: "hide"
        });
    }
    this.showNavigation = function () {
        $rootScope.$broadcast("NavigationBar", {
            set: "show"
        });
    }
    this.toggleNavigation = function () {
        $rootScope.$broadcast("NavigationBar", {
            toggle: true
        });
    }
    //Status Bar
    this.showStatusBar = function () {
        StatusBar.show();
    }
    this.hideStatusBar = function () {
        StatusBar.hide();
    }

    //Hub Update
    this.hub.client.update = function (state) {
        $rootScope.$broadcast("hub_onStateUpdate", state);
    };

    //Login Methods
    this.loginToFacebook = function (callback, callbackError) {
        facebookConnectPlugin.getLoginStatus(function (success) {
            if (success.status != "connected") {
                return loginToFacebookSDK(callback, callbackError);
            } else {
                facebookAuth = success.authResponse;
                callback(facebookAuth);
                return facebookAuth;
            }
        }, function (error) {
            return loginToFacebookSDK(callback, callbackError);
        });

    };
    function loginToFacebookSDK(callback, callbackError) {
        facebookConnectPlugin.login(["email", "user_friends"], function (success) {
            facebookAuth = success.authResponse;
            callback(facebookAuth);
            return facebookAuth;
        }, function (error) {
            callbackError();
            return null;
        });
    };
    this.loginToGameServer = function (callback, callbackError) {
        if (scope.loggedIn) {
            return;
        }
        $http({
            url: scope.baseUrl + "api/Account/RegisterExternalToken",
            method: "POST",
            data: {
                Token: facebookAuth.accessToken,
                Provider: "Facebook"
            }
        }).then(
            function (success) {
                $http.defaults.headers.common.Authorization = "Bearer " + success.data.access_token;
                localStorage.setObject("appAuth", success.data);
                scope.loggedIn = true;
                scope.accessData = success.data;
                callback(scope.accessData);
            },
            function (error) {
                facebookConnectPlugin.logout();
                scope.loggedIn = false;
                callbackError(error);
            });
    };
    this.checkLogin = function (callback, callbackError) {
        $http({
            url: scope.baseUrl + "api/Account/Check",
            method: "GET",
        }).then(callback, callbackError);
    }
    this.loadUserData = function (callback, callbackError) {
        $http({
            url: scope.baseUrl + "api/Account/UserInfo",
            method: "GET"
        }).then(function (success) {
            scope.userInfo = success.data;
            callback(scope.userInfo);
        }, function (error) {
            callbackError(error);
        });
    };
    this.connectToLobby = function (callback, callbackError) {
        if (scope.accessData == null || scope.accessData.access_token == null) {
            callbackError(new NotAuthorizedException());
        } else {
            var listener = $rootScope.$on("hub_onStateUpdate", function (event, state) {
                if (state.Event != null && state.Event.Connected == true) {
                    scope.hubConnected = true;
                    callback(scope.hub);
                    listener();
                } else if (state.Event != null && state.Event.Error != null && state.Event.Connected == false) {
                    scope.hubConnected = false;
                    callbackError(state.Error);
                    listener();
                }
            });
            $.connection.hub.start(function () {
                scope.hub.server.login(scope.accessData.access_token);
            });
            return true;
        }
    };

    this.listRunningGames = function (filter, callback, callbackError) {
        $http({
            url: scope.baseUrl + "api/Game/ListInProgress" + (filter == null ? "" : ("?gameType=" + filter)),
            method: "GET"
        }).then(function (success) {
            callback(success.data);
        }, function (error) {
            callbackError(error);
        });
    }

    this.listGames = function (filter, callback, callbackError) {
        $http({
            url: scope.baseUrl + "api/Game/List" + (filter == null ? "" : ("?gameType=" + filter)),
            method: "GET"
        }).then(function (success) {
            callback(success.data);
        }, function (error) {
            callbackError(error);
        });
    }
    this.logout = function () {
        scope.loggedIn = false;
        $http.defaults.headers.common.Authorization = null;
        scope.accessData = null;
        facebookConnectPlugin.logout();R
        localStorage.setObject("appAuth", null);
    };

});

function NotAuthorizedException() {
    this.message = "User Not Authorized To Connect To SignalR Hub!";
    this.friendlyMessage = "User Not Authorized! Please Login / Try Again."
    this.toString = function () {
        return this.friendlyMessage;
    };
}