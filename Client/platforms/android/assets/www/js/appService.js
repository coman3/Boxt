app.service("BoxtService", function ($rootScope, $http) {
    var scope = this;
    var facebookAuth = null;

    this.baseUrl = "http://192.168.0.17/";
    this.accessData = null;
    this.loggedIn = false;
    this.userInfo = null;
    this.hub = null;
    this.backPressAction = function (event) { event.preventDefault = true; }
    hub = $.connection.lobbyHub;
    lobbyHub.client.serverMessage = function (message) {
        console.log(message);
        $rootScope.$broadcast("hub_onServerMessage", message);
    };
    lobbyHub.client.update = function (state) {
        console.log(state);
        $rootScope.$broadcast("hub_onStateUpdate", state);
    };

    if (localStorage.getObject("appAuth") != null) {
        this.accessData = localStorage.getObject("appAuth");
        this.loggedIn = true;
    }

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
    }

    this.loginToGameServer = function (callback, callbackError) {
        if (scope.loggedIn) {
            return;
        }
        $http({
            url: baseUrl + "api/Account/RegisterExternalToken",
            method: "POST",
            data: {
                Token: authResponse.accessToken,
                Provider: "Facebook"
            }
        }).then(
            function (success) {
                $http.defaults.headers.common.Authorization = "Bearer " + success.access_token;
                localStorage.setObject("appAuth", success)
                scope.loggedIn = true;
                auth = success;
                callback(auth);
            },
            function (error) {
                facebookConnectPlugin.logout();
                scope.loggedIn = false;
                callbackError();
            });
    }
    this.connectToLobby = function (callback, callbackError) {
        if (scope.accessData == null || scope.accessData.access_token == null) {
            callbackError(new NotAuthorizedException());
        } else {
            $.connection.hub.start(function () {
                lobbyHub.server.login(scope.accessData.access_token);
                callback(scope.lobbyHub);
            });
            return true;
        }
    };


});

function NotAuthorizedException() {
    this.message = "User Not Authorized To Connect To SignalR Hub!";
    this.friendlyMessage = "User Not Authorized! Please Login / Try Again."
    this.toString = function () {
        return this.value + this.message;
    };
}