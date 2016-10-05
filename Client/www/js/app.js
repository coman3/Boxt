var backButtonAction = function (e) {

}
document.addEventListener("backbutton", function (e) {
    e.preventDefault();
    backButtonAction();
}, false);


Storage.prototype.setObject = function (key, value) { this.setItem(key, JSON.stringify(value)); }
Storage.prototype.getObject = function (key) { var value = this.getItem(key); return value && JSON.parse(value); }

var app = angular.module('app', ['ngMaterial'])
.config(function($mdThemingProvider) {
  $mdThemingProvider.theme('default')
    .primaryPalette('green')
    .accentPalette('orange');
});;
app.controller("Loader", function ($scope) {
    $scope.loaderMessage = "Loading...";

    setTimeout(load, 1000);
    function load() {
        enterFullscreen();
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
        window.location.href = "login.html";
    }
    function appLogin(authResponse) {
        updateMessage("Logging into game server...")
        $.ajax({
            url: "http://192.168.0.17:10823/api/Account/RegisterExternalToken",
            method: "POST",
            data: {
                Token: authResponse.accessToken,
                Provider: "Facebook"
            },
            success: function (success) {
                localStorage.setObject("facebookAuth", null)
                localStorage.setObject("appAuth", success)
                updateMessage("Logged In!")
                window.location.href = "app.html";
            },
            error: function (error) {
                alert("An error occurred while trying to login, please restart and try again.")
            }
        })
    }

});

app.controller("Login", function ($scope, $mdDialog) {
    $scope.facebookLogin = function (ev) {
        var dialog = $mdDialog.show({
            contentElement: '#AuthPopup',
            parent: angular.element(document.body),
            targetEvent: ev,
        });

        facebookConnectPlugin.login(["email"], function (success) {
            window.location = "index.html";
        }, function (error) {

        });

    }
});
app.controller("SideNav", function ($scope) {
    $scope.signOut = function(){
        facebookConnectPlugin.logout();
        localStorage.setObject("auth", null)
        window.location.href = "index.html"
    }
});

app.controller("MainApp", function ($scope, $timeout, $mdSidenav) {
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