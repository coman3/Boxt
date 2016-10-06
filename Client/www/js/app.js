app.controller("Loader", function ($scope, $location, $window) {
    $scope.loaderMessage = "Loading...";

    setTimeout(load, 500);
    function load() {
        if (typeof StatusBar == null) {
            checkFacebookAuth();
        } else {
            enterFullscreen();
        }
    }

    function updateMessage(message) {
        $scope.loaderMessage = message;
        $scope.$apply()
    }

    function enterFullscreen() {
        if (StatusBar.isVisible) {
            StatusBar.hide();
            updateMessage("Entering Fullscreen...");
        }
        checkFacebookAuth();
    }
    function checkFacebookAuth() {
        updateMessage("Authenticating...")
        facebookConnectPlugin.getLoginStatus(function (success) {
            if (success.status != "connected") {
                setTimeout(loginStart, 500);
            } else {
                updateMessage("Social Authentication Successful!")
                localStorage.setObject("facebookAuth", success.authResponse)
                appLogin(success.authResponse);
            }
        }, function (error) {
            updateMessage("Authentication Error!")
            setTimeout(loginStart, 500);
        });
    }
    function loginStart() {
        $location.path("/login");
        $scope.$apply()
    }
    function appLogin(authResponse) {
        updateMessage("Logging into game server...")
        $.ajax({
            url: "http://textit.coman3.xyz/api/Account/RegisterExternalToken",
            method: "POST",
            data: {
                Token: authResponse.accessToken,
                Provider: "Facebook"
            },
            success: function (success) {
                localStorage.setObject("facebookAuth", null)
                localStorage.setObject("appAuth", success)
                updateMessage("Logged In!")
                $location.path("/lobby");
                $scope.$apply()

            },
            error: function (error) {
                alert("An error occurred while trying to login, trying again.")
                facebookConnectPlugin.logout();
                $window.location.reload();
                $scope.$apply()
            }
        })
    }

});
app.controller("Login", function ($scope, $mdDialog, $location) {
    $scope.facebookLogin = function (ev) {
        $mdDialog.show({
            contentElement: '#AuthPopup',
            parent: angular.element(document.body),
            targetEvent: ev,
        });

        facebookConnectPlugin.login(["email", "user_friends"], function (success) {
            $location.path("/");
            $scope.$apply()
        }, function (error) {
            alert("Authentication Failed!");
            $mdDialog.hide();
        });

    }
});
app.controller("SideNav", function ($scope, $location) {
    $scope.signOut = function () {
        facebookConnectPlugin.logout();
        localStorage.setObject("auth", null)
        $location.path("/");
        $scope.$apply()
    }
    $scope.share = function () {
        var options = {
            message: 'share this', // not supported on some apps (Facebook, Instagram)
            url: 'http://www.textit.coman3.xyz/',
            chooserTitle: 'Pick an app' // Android only, you can override the default share sheet title
        }

        var onSuccess = function (result) {
            console.log("Share completed? " + result.completed); // On Android apps mostly return false even while it's true
            console.log("Shared to app: " + result.app); // On Android result.app is currently empty. On iOS it's empty when sharing is cancelled (result.completed=false)
        }

        var onError = function (msg) {
            console.log("Sharing failed with message: " + msg);
        }

        window.plugins.socialsharing.shareWithOptions(options, onSuccess, onError);
    }
});
app.controller("Lobby", function ($scope, $timeout, $mdSidenav) {
    backButtonAction = function () {
        $mdSidenav('left').toggle();
    };
    $scope.toggleLeft = backButtonAction;
    $scope.tiles = [
        {
            icon: "img/tictactoe.svg",
            title: "tick tac toe",
            style: {
                'background-color': "#FF3300",
            },
            span: {
                row: 1,
                col: 2
            }
        }, {
            icon: "img/tictactoe.svg",
            title: "4 in a row",
            style: {
                'background-color': "#FF3300",
            },
            span: {
                row: 1,
                col: 1
            }
        }, {
            icon: "img/tictactoe.svg",
            title: "guess the word",
            style: {
                'background-color': "#FF3300",
            },
            span: {
                row: 2,
                col: 1
            }
        }, {
            icon: "img/tictactoe.svg",
            title: "guess the drawing",
            style: {
                'background-color': "#FF3300",
            },
            span: {
                row: 1,
                col: 1
            }
        }, {
            icon: "img/tictactoe.svg",
            title: "popular game 1",
            style: {
                'background-color': "#FF3300",
            },
            span: {
                row: 1,
                col: 3
            }
        },
    ];
    var imagePath = "https://placeholdit.imgix.net/~text?txtsize=8&txt=64%C3%9764&w=64&h=64";
    $scope.todos = [
        {
            face: imagePath,
            what: 'Brunch this weekend?',
            who: 'Min Li Chan',
            when: '3:08PM',
            notes: " I'll be in your neighborhood doing errands",
            action: "play_arrow"
        },
        {
            face: imagePath,
            what: 'Brunch this weekend?',
            who: 'Min Li Chan',
            when: '3:08PM',
            notes: " I'll be in your neighborhood doing errands",
            action: "play_arrow"
        },
        {
            face: imagePath,
            what: 'Brunch this weekend?',
            who: 'Min Li Chan',
            when: '3:08PM',
            notes: " I'll be in your neighborhood doing errands",
            action: "play_arrow"
        },
        {
            face: imagePath,
            what: 'Brunch this weekend?',
            who: 'Min Li Chan',
            when: '3:08PM',
            notes: " I'll be in your neighborhood doing errands",
            action: "play_arrow"
        },
        {
            face: imagePath,
            what: 'Brunch this weekend?',
            who: 'Min Li Chan',
            when: '3:08PM',
            notes: " I'll be in your neighborhood doing errands",
            action: "play_arrow"
        },
    ];
});