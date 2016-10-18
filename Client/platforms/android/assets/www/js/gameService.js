app.service("BoxtGameService", function ($rootScope, $http, $timeout) {
    var scope = this;
    this.Invites = [];
    this.Games = [];
    this.GameTypes = [];
    this.PopularGames = this.GameTypes;
    this.FeaturedGames = this.GameTypes;
    this.OnCreateInvite = function (invite) {
        var userId = $rootScope.BoxtService.getUserId();
        if (invite.Inviter.Id = userId) return;
        scope.Invites.push(invite);
    };
    this.OnAcceptInvite = function (invite) {
        scope.Games.push(invite.Game);
    };
    this.getGame = function (gameId) {
        for (var i = 0; i < scope.GameTypes.length; i++) {
            var game = scope.GameTypes[i];
            if (game.Id == gameId) {
                return game;
            }
        }
    }
    this.LoadAll = function (callback, callbackError) {
        //remove all items from arrays
        scope.Games.splice(0, scope.Games.length);
        scope.GameTypes.splice(0, scope.GameTypes.length);
        scope.Invites.splice(0, scope.Invites.length);

        listAllGameTypes(function (success) {
            success.forEach(function (item) {
                scope.GameTypes.push(item);
            });
            listGameInvites(function (success) {
                success.forEach(function (item) {
                    scope.Invites.push(item);
                });
                listUserGames(null, function (success) {
                    success.forEach(function (item) {
                        scope.Games.push(item);
                    });
                    callback();
                }, function (error) {
                    callbackError(error);
                });
            }, function (error) {
                callbackError(error);
            });
        }, function (error) {
            callbackError(error);
        });
    }

    //List Games
    function listRunningGames(filter, callback, callbackError) {
        $http({
            url: $rootScope.BoxtService.baseUrl + "api/Game/ListInProgress" + (filter == null ? "" : ("?gameType=" + filter)),
            method: "GET"
        }).then(function (success) {
            callback(success.data);
        }, function (error) {
            callbackError(error);
        });
    };
    function listUserGames(filter, callback, callbackError) {
        $http({
            url: $rootScope.BoxtService.baseUrl + "api/Game/List" + (filter == null ? "" : ("?gameType=" + filter)),
            method: "GET"
        }).then(function (success) {
            callback(success.data);
        }, function (error) {
            callbackError(error);
        });
    };
    function listAllGameTypes(callback, callbackError) {
        $http({
            url: $rootScope.BoxtService.baseUrl + "api/Game/ListType",
            method: "GET"
        }).then(function (success) {
            scope.games = success.data;
            callback(success.data);
        }, function (error) {
            callbackError(error);
        });
    };
    function listGameInvites(callback, callbackError) {
        $http({
            url: $rootScope.BoxtService.baseUrl + "api/Invite/List",
            method: "GET"
        }).then(function (success) {
            callback(success.data);
        }, function (error) {
            callbackError(error);
        });
    };

    this.createGame = function (gameApplicationId, callback, callbackError) {
        $http({
            url: $rootScope.BoxtService.baseUrl + "api/Game/Create?gameApplicationId=" + gameApplicationId,
            method: "GET"
        }).then(function (success) {
            callback(success.data);
        }, function (error) {
            callbackError(error);
        });
    };

    this.inviteUserToGame = function (gameId, userId, callback, callbackError) {
        $http({
            url: $rootScope.BoxtService.baseUrl + "api/Invite/Invite?gameId=" + gameId + "&inviteUserId=" + userId,
            method: "GET"
        }).then(function (success) {
            callback(success.data);
        }, function (error) {
            callbackError(error);
        });
    };

    this.acceptInvite = function (inviteId, callback, callbackError) {
        $http({
            url: $rootScope.BoxtService.baseUrl + "api/Invite/Accept?inviteId=" + inviteId,
            method: "GET"
        }).then(function (success) {
            callback(success.data);
        }, function (error) {
            callbackError(error);
        });
    };

    this.getGameState = function (gameId, callback, callbackError) {
        $http({
            url: $rootScope.BoxtService.baseUrl + "api/Game/State?gameId=" + gameId,
            method: "GET"
        }).then(function (success) {
            callback(success.data);
        }, function (error) {
            callbackError(error);
        });
    };


});