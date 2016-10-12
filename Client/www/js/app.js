app.controller("Loader", function ($scope, $location, $window) {
    $scope.loaderMessage = "Loading...";
    //common
    function updateMessage(message) {
        console.log(message);
    }
    $scope.$on('$viewContentLoaded', function () {
        document.addEventListener("deviceready", function () {
            if (localStorage.getObject("appAuth") != null) {
                auth = localStorage.getObject("appAuth");
                updateMessage("Logging in using saved credentials...");
                loginToSignalR();
            } else {
                loginToFacebook();
            }

        }, false);
    });

    //loading
    function loginToFacebook() {
        updateMessage("Authenticating...")
        facebookConnectPlugin.getLoginStatus(function (success) {
            if (success.status != "connected") {
                $location.path("/login");
                $scope.$apply();
            } else {
                updateMessage("Social Authentication Successful!")
                loginToGameServer(success.authResponse);
            }
        }, function (error) {
            updateMessage("Authentication Error!")
            $location.path("/login");
            $scope.$apply()
        });
    }
    function loginToGameServer(authResponse) {
        updateMessage("Logging into game server...")
        $.ajax({
            url: hostAddress + "api/Account/RegisterExternalToken",
            method: "POST",
            data: {
                Token: authResponse.accessToken,
                Provider: "Facebook"
            },
            success: function (success) {
                localStorage.setObject("appAuth", success)
                auth = success;
                updateMessage("Connecting To SignalR Server...")
                loginToSignalR();

            },
            error: function (error) {
                alert("An error occurred while trying to login. Please try again.")
                facebookConnectPlugin.logout();
                $window.location.reload();
                $scope.$apply()
            }
        })
    }
    function loginToSignalR() {
        lobbyHub = $.connection.lobbyHub;
        lobbyHub.client.serverMessage = function (message) {
            console.log(message);
        };
        $.connection.hub.start(function () {
            lobbyHub.server.login(auth.access_token);
            updateMessage("Loading User Data...")
            getUserData();
        });
    }
    function getUserData() {
        $.ajax({
            url: hostAddress + "api/Account/UserInfo",
            method: "GET",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + auth.access_token)
            },
            success: function (success) {
                updateMessage("Starting Game!")
                $location.path("/lobby");
                $scope.$apply()
            },
            error: function (error) {
                alert("An error occurred while trying to collect user data. Please try again.")
                facebookConnectPlugin.logout();
                $window.location.reload();
                $scope.$apply()
            }
        });
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
        localStorage.setObject("appAuth", null)
        auth = null;
        $location.path("/");
        $scope.$apply()
    }
    $scope.TicTacToe = function () {
        $location.path("games/TicTacToe");
    }
    $scope.share = function () {
        var options = {
            message: 'share this', // not supported on some apps (Facebook, Instagram)
            url: 'http://boxt.coman3.xyz/',
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
    $scope.$on("$viewContentLoaded", function () {
        if (StatusBar.isVisible) {
            StatusBar.hide();
        }

    });

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

app.controller("TicTacToe", function ($scope, $location) {
    backButtonAction = function () {
        $location.path("/lobby");
        $scope.$apply();
    };
    $scope.toggle = function (index) {
        var currentValue = $scope.board[index].value;
        var nextValue = "";
        if (typeof currentValue == null) return;

        if (currentValue == "o") {
            nextValue = "x"
        } else if (currentValue == "") {
            nextValue = "o"
        }
        lobbyHub.server.update(index, nextValue);
    }
    $scope.board = {
        Items: []
    };
    $scope.sendUpdate = function (index) {
        lobbyProxy.server.updateGame(gameId,
            {
                turn: {
                    index: index,
                }
            });
    }
    $scope.load = function () {
        lobbyHub.on("update", function (state) {
            console.log(state);
            if (state.turn != null) {
                $scope.board.Items[state.turn.index].Value = state.turn.value;
            }
            if (state.GameEnd != null) {
                alert(state.GameEnd.Reason);
            } $scope.$apply();
        });
        $.ajax({
            url: hostAddress + "api/Game/State?gameId=" + gameId,
            method: "GET",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + auth.access_token);
            },
            success: function (success) {
                console.log(success);

                success.Board.Items.forEach(function (item) {
                    $scope.board.Items.push(item);
                });
                $scope.$apply();
                lobbyProxy.server.login(auth.access_token);
                lobbyProxy.server.joinGame(success.Game.Id);
            },
            error: function (error) {
                alert("An error occurred while loading game, please try again.")
                $location.path("/lobby");
                $scope.$apply();
            }
        });
    }
});


//new

app.controller("LeftNavigation",
    ["scope", "location", "BoxtService", "mdSidenav",
        function ($scope, $location, $BoxtService, $mdSideNav) {
            $scope.MenuTitle = "Menu";

            $scope.MenuItems = [
                {
                    GroupTitle: "Browse",
                    Items: [
                        {
                            Title: "Featured",
                            Click: function(){
                                alert("Featured");
                            },
                            Icon: {
                                Type: "FA",
                                Icon: "trophy"  
                            }
                        },
                        {
                            Title: "Popular",
                            Click: function(){
                                alert("Popular");
                            },
                            Icon: {
                                Type: "FA",
                                Icon: "line-chart"  
                            }
                        },
                        {
                            Title: "Favorites",
                            Click: function(){
                                alert("Favorites");
                            },
                            Icon: {
                                Type: "MD",
                                Icon: "star"  
                            }
                        }
                    ]
                }
            ];

            $scope.navigate = function (location) {
                $location.path("/" + location);
            }

            $scope.toggleNav = function(){
                $mdSidenav('left').toggle();
            };
        }]);
app.controller("TopNavigation",
    ["scope", "location", "BoxtService", "mdSidenav",
        function ($scope, $location, $Boxt, $mdSideNav) {
            $scope.NavigationTitle = "Game Lobby";
            $scope.toggleNav = function(){
                $mdSidenav('left').toggle();
            };
        }]);