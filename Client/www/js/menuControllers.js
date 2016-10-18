app.controller("menu/friends", ["$rootScope", "$scope", "$mdSidenav", "BoxtService", "$routeParams", "$http", Menu_FriendsController]);
app.controller("menu/games", ["$rootScope", "$scope", "$mdSidenav", "BoxtService", "BoxtGameService", "$routeParams", "$timeout", Menu_GamesController]);
app.controller("menu/invite", ["$rootScope", "$scope", "$mdSidenav", "BoxtService", "$routeParams", "$timeout", Menu_InviteController]);
app.controller("menu/create", ["$rootScope", "$scope", "$mdSidenav", "BoxtService", "$routeParams", "$timeout", Menu_CreateController]);

function Menu_FriendsController($rootScope, $scope, $mdSidenav, $BoxtService, $routeParams, $http) {
    var cachedQuery, lastSearch;
    $scope.friends = [];
    $scope.allFriends = [];
    $scope.filterSelected = true;
    $BoxtService.loadUserFriends(function (success) {
        success.forEach(function (item) {
            item._lowername = item.Name.toLowerCase();
            $scope.allFriends.push(item);
        });
    }, function (error) {

    });

    $scope.querySearch = function querySearch(criteria) {
        cachedQuery = cachedQuery || criteria;
        return cachedQuery ? $scope.allFriends.filter(createFilterFor(cachedQuery)) : [];
    }
    function createFilterFor(query) {
        var lowercaseQuery = angular.lowercase(query);

        return function filterFn(contact) {
            return (contact._lowername.indexOf(lowercaseQuery) != -1);;
        };

    }
    $scope.addFriend = function (friend) {
        if ($scope.friends.indexOf(friend) < 0) {
            $scope.friends.push(friend);
        }
    }
    $scope.removeFriend = function (friend) {
        var index = $scope.friends.indexOf(friend);
        $scope.friends.splice(index, 1);
    }
};

function Menu_GamesController($rootScope, $scope, $mdSidenav, $BoxtService, $BoxtGameService, $routeParams, $timeout) {
    $scope.games = $BoxtGameService.GameTypes;
    $scope.game = null;
    $scope.subHeader = "Games";
    var param = $routeParams.filter;
    switch (param) {
        case "all":
        case "":
        case null:
            $scope.subHeader = "All Games";
            break;
        default:
            $scope.games = null;
            $scope.game = $BoxtGameService.getGame(parseInt(param))
            $scope.subHeader = $scope.game.Name;
            break;
    };

    $scope.gridItemClick = function (item) {
        $BoxtService.navigate("menu/games/" + item.Id);
    };
    $scope.selectInviteFriend = function (friends) {
        var game = $scope.game;
        if (game.MaxPlayers - 1 > friends.length && game.MaxPlayers != 2) {
            alert("To many players invited, please invite a maximum of " + (game.MaxPlayers - 1) + " players!")
            return;
        }
        if (game.MinPlayers - 1 < friends.length) {
            alert("To few players invited, please invite a minimum of " + (game.MinPlayers - 1) + " players!")
            return;
        }

        if (game.MaxPlayers == 2) {
            friends.forEach(function (friend) {
                $BoxtGameService.createGame(game.Id, function (success) {
                    $BoxtGameService.inviteUserToGame(success.Id, friend.Id, function (success) {
                        $BoxtService.navigate("");
                    }, function (error) {
                        alert(error);
                    });
                }, function (error) {
                    alert(error);
                });
            });
        } else {
            $BoxtGameService.createGame(game.Id, function (success) {
                friends.forEach(function (friend) {
                    $BoxtGameService.inviteUserToGame(success.Id, friend.Id, function (success) {
                        $BoxtService.navigate("");
                    }, function (error) {
                        alert(error);
                    });
                })
            }, function (error) {
                alert(error);
            });
        }



    };
};

function Menu_InviteController($rootScope, $scope, $mdSidenav, $BoxtService, $mdDialog, $http) {

};

function Menu_CreateController($rootScope, $scope, $mdSidenav, $BoxtService, $mdDialog, $http) {

};