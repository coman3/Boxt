app.controller("NavigationController", ["$rootScope", "$scope", "$location", "$mdSidenav", "BoxtService", NavigationController]);
app.controller("Loader", ["$rootScope", "$scope", "$location", "$mdSidenav", "BoxtService", "$mdDialog", "$timeout", "BoxtGameService", LoaderController]);
app.controller("Lobby", ["$rootScope", "$scope", "$location", "$mdSidenav", "BoxtService", "BoxtGameService", "$mdDialog", "$timeout", LobbyController]);

function NavigationController($rootScope, $scope, $location, $mdSidenav, $BoxtService) {
    $scope.MenuTitle = "Boxt";
    $scope.NavigationTitle = "Boxt";
    $scope.showNavigation = false;
    $rootScope.$on("NavigationBar", function (event, args) {
        if (args.set != null) {
            $scope.showNavigation = (args.set == "show");
        } else if (args.toggle == true) {
            $scope.showNavigation = !$scope.showNavigation;
        }
    });
    $rootScope.$on("NavigationBarLeft", function (event, args) {
        if (!$scope.showNavigation)
            return;

        if (args.show) {
            $mdSidenav('left').open();
        } else if (args.hide) {
            $mdSidenav('left').close();
        } else {
            $mdSidenav('left').toggle();
        }
    });
    backButtonAction = function () {
        if ($scope.showNavigation == true) {
            $mdSidenav('left').toggle();
        }
    };

    $scope.MenuItems = [
        {
            Items: [
                {
                    Title: "Home",
                    Click: function () {
                        $BoxtService.navigate("lobby");
                    },
                    Icon: {
                        Type: "MD",
                        Icon: "home"
                    }
                }
            ]
        },
        {
            GroupTitle: "Browse",
            Items: [
                {
                    Title: "Featured",
                    Click: function () {
                        $BoxtService.navigate("menu/games/featured");
                    },
                    Icon: {
                        Type: "FA",
                        Icon: "trophy"
                    }
                },
                {
                    Title: "Popular",
                    Click: function () {
                        $BoxtService.navigate("menu/games/popular");
                    },
                    Icon: {
                        Type: "FA",
                        Icon: "line-chart"
                    }
                },
                {
                    Title: "All",
                    Click: function () {
                        $BoxtService.navigate("menu/games/all");
                    },
                    Icon: {
                        Type: "MD",
                        Icon: "local_mall"
                    }
                }
            ]
        },
        {
            GroupTitle: "Account",
            Items: [
                {
                    Title: "Friends",
                    Click: function () {
                        $BoxtService.navigate("menu/friends/all");
                    },
                    Icon: {
                        Type: "MD",
                        Icon: "people"
                    }
                },
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

    $scope.getUserInfo = function () {
        return $BoxtService.userInfo;
    };

    $scope.toggleNav = function () {
        if ($BoxtService.navDisabled)
            return;

        $mdSidenav('left').toggle();
    };
};

function LoaderController($rootScope, $scope, $location, $mdSidenav, $BoxtService, $mdDialog, $timeout, $BoxtGameService) {
    $scope.loaderMessage = "Loading...";
    $BoxtService.hideNavigation();
    //common
    if ($BoxtService.loggedIn) {
        checkLogin();
    } else {
        loginWithFacebook();
    }
    function loginWithFacebook() {
        var confirm = $mdDialog.confirm()
            .title("Login?")
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
};

function LobbyController($rootScope, $scope, $location, $mdSidenav, $BoxtService, $BoxtGameService, $mdDialog, $timeout) {

    $scope.BoxtService = $BoxtService;
    $scope.toggleLeft = backButtonAction;
    $scope.featuredTiles = $BoxtGameService.FeaturedGames;
    $scope.popularTiles = $BoxtGameService.PopularGames;
    $scope.games = $BoxtGameService.Games;
    $scope.gameInvites = $BoxtGameService.Invites;
    $scope.gameItemClick = function (item) {
        $BoxtService.navigate("menu/games/" + item.Id);
    };

    $scope.GameInProgressClick = function (game) {
        $BoxtService.navigate("games/" + game.GameApplication.Id + "/" + game.Id);
    }

    $scope.acceptInvite = function (invite) {
        $BoxtGameService.acceptInvite(invite.Id, function (success) {
            console.log(success);
        }, function (error) {
            console.log(error);
        });
    };

};


