app.controller("Loader", ["$rootScope", "$scope", "$location", "$mdSidenav", "BoxtService", "$mdDialog", "$timeout",
    function ($rootScope, $scope, $location, $mdSidenav, $BoxtService, $mdDialog, $timeout) {
        $scope.loaderMessage = "Loading...";
        //common
        if ($BoxtService.loggedIn) {
            checkLogin();
        } else {
            loginWithFacebook();
        }
        function loginWithFacebook() {
            var confirm = $mdDialog.confirm()
                .title("Login With Facebook?")
                .textContent("Hello, to use Boxt you must login with facebook.")
                .ariaLabel("Login")
                .ok("Login With Facebook")
                .cancel("No Thanks");
            $mdDialog.show(confirm).then(function (confirm) {
                $scope.loaderMessage = "Logging in with facebook...";
                $BoxtService.loginToFacebook(function (success) {
                    $scope.loaderMessage = "Logging into Game Server...";
                    $BoxtService.loginToGameServer(function (success) {
                        getUserData();
                    }, function (error) {
                        $scope.loaderMessage = "Login To Game Server Failed. Trying Again...";
                        tryAgain();
                    });
                }, function (error) {
                    $scope.loaderMessage = "Login Failed, Trying Again...";
                    tryAgain();
                });
            }, function (cancel) {
                $scope.loaderMessage = "Please Login To Use Boxt.";
                tryAgain();
            });
        }
        function checkLogin() {
            $BoxtService.checkLogin(function (success) {
                getUserData();
            }, function (error) {
                $BoxtService.logout();
                loginWithFacebook();
            })
        }

        function getUserData() {
            $scope.loaderMessage = "Loading Data...";
            $BoxtService.loadUserData(function (success) {
                $scope.loaderMessage = "Connecting To Lobby..."
                $BoxtService.connectToLobby(function (success) {
                    $scope.loaderMessage = "Starting...";
                    $scope.$apply();
                    $BoxtService.showNavigation();
                    $BoxtService.navigate("lobby");
                    $timeout(function () {
                        $scope.$apply();
                    }, 500);
                }, function (error) {
                    $scope.loaderMessage = "Failed Connecting To Hub. Trying Again...";
                    tryAgain();
                });
            }, function (error) {
                $scope.loaderMessage = "Failed Getting User Data. Trying Again...";
                tryAgain();
            });
        }

        function tryAgain() {
            $timeout(function () {
                window.location.reload();
            }, 5000);
        }
    }]);

app.controller("Lobby", ["$rootScope", "$scope", "$location", "$mdSidenav", "BoxtService", "$mdDialog", "$timeout",
    function ($rootScope, $scope, $location, $mdSidenav, $BoxtService, $mdDialog, $timeout) {

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

        $scope.GamesInProgress = [];
        $BoxtService.listGames(null, function (success) {
            success.forEach(function (item) {
                $scope.GamesInProgress.push(item);
            });
        }, function (error) {
            console.error(error);
        })
    }]);



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

app.controller("NavigationController",
    ["$rootScope", "$scope", "$location", "$mdSidenav", "BoxtService",
        function ($rootScope, $scope, $location, $mdSidenav, $BoxtService) {
            $scope.MenuTitle = "Menu";
            $scope.NavigationTitle = "Game Lobby";
            $scope.showNavigation = false;
            $rootScope.$on("NavigationBar", function (event, args) {
                if (args.set != null) {
                    $scope.showNavigation = (args.set == "show");
                } else if (args.toggle == true) {
                    $scope.showNavigation = !$scope.showNavigation;
                }
            });
            backButtonAction = function () {
                if ($scope.showNavigation == true) {
                    $mdSidenav('left').toggle();
                }
            };

            $scope.MenuItems = [
                {
                    GroupTitle: "Browse",
                    Items: [
                        {
                            Title: "Featured",
                            Click: function () {
                                alert("Featured");
                            },
                            Icon: {
                                Type: "FA",
                                Icon: "trophy"
                            }
                        },
                        {
                            Title: "Popular",
                            Click: function () {
                                alert("Popular");
                            },
                            Icon: {
                                Type: "FA",
                                Icon: "line-chart"
                            }
                        },
                        {
                            Title: "Favorites",
                            Click: function () {
                                alert("Favorites");
                            },
                            Icon: {
                                Type: "MD",
                                Icon: "star"
                            }
                        }
                    ]
                },
                {
                    GroupTitle: "Account",
                    Items: [
                        {
                            Title: "Settings",
                            Click: function () {
                                alert("Settings");
                            },
                            Icon: {
                                Type: "MD",
                                Icon: "settings"
                            }
                        },
                        {
                            Title: "About",
                            Click: function () {
                                alert("About");
                            },
                            Icon: {
                                Type: "MD",
                                Icon: "info_outline"
                            }
                        },
                        {
                            Title: "Sign Out",
                            Click: function () {
                                $BoxtService.logout();
                                $BoxtService.hideNavigation();
                                $BoxtService.navigate("");
                            },
                            Icon: {
                                Type: "FA",
                                Icon: "sign-out"
                            }
                        }
                    ]
                }
            ];

            $scope.toggleNav = function () {
                if ($BoxtService.navDisabled)
                    return;

                $mdSidenav('left').toggle();
            };
        }]);