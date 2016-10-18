app.controller("menu/friends", ["$rootScope", "$scope", "$location", "$mdSidenav", "BoxtService", "$routeParams", "$http", Menu_FriendsController]);
app.controller("menu/games", ["$rootScope", "$scope", "$location", "$mdSidenav", "BoxtService", "$routeParams", "$timeout", Menu_GamesController]);
app.controller("menu/invite", ["$rootScope", "$scope", "$location", "$mdSidenav", "BoxtService", "$routeParams", "$timeout", Menu_InviteController]);
app.controller("menu/create", ["$rootScope", "$scope", "$location", "$mdSidenav", "BoxtService", "$routeParams", "$timeout", Menu_CreateController]);

function Menu_FriendsController($rootScope, $scope, $location, $mdSidenav, $BoxtService, $routeParams, $http) {
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

function Menu_GamesController($rootScope, $scope, $location, $mdSidenav, $BoxtService, $routeParams, $timeout) {
    $scope.games = [];
    $scope.game = null;
    $scope.subHeader = "Games";
    var param = $routeParams.filter;
    switch (param) {
        case "all":
            $scope.subHeader = "All Games";
            $scope.game = $BoxtService.listAllGames(function (success) {
                for (var i = 0; i < success.length; i++) {
                    $scope.games.push(success[i]);
                }
            }, function (error) {
                console.error(error);
            });
            break;
        default:
            $scope.games = null;
            $scope.game = $BoxtService.getGame(parseInt(param))
            break;
    };

    $scope.gridItemClick = function (item) {
        $BoxtService.navigate("menu/games/" + item.Id);
    };
    $scope.selectInviteFriend = function (friend) {
        $BoxtService.createGame($scope.game.Id, function (success) {
            console.log(success);
            $BoxtService.inviteUserToGame(success.Id, friend.Id, function (success) {
                console.log(success)
            }, function (error) {

            });
        }, function (error) {

        });
    };
};

function Menu_InviteController($rootScope, $scope, $location, $mdSidenav, $BoxtService, $mdDialog, $http) {

};

function Menu_CreateController($rootScope, $scope, $location, $mdSidenav, $BoxtService, $mdDialog, $http) {

};