app.controller("GameController_1", ["$rootScope", "$routeParams", "$scope", "BoxtService", TicTacToeController]);


function TicTacToeController($rootScope, $routeParams, $scope, $BoxtService) {
    var gameId = $routeParams.gameId;
    $scope.board = {
        Items: []
    };
    $scope.state = null;
    $rootScope.$on("hub_onStateUpdate", function (event, state) {
        if (state.turn != null) {
            $scope.board.Items[state.turn.index].Value = state.turn.value;
        }
        if (state.GameEnd != null && state.GameEnd.Id == gameId) {
            $timeout(function () {
                $scope.board.Items[state.turn.index].forEach(function (item) {
                    item.Value = 0;
                });
            }, 3000);
            alert("Game Ended!");
        }
        $scope.$apply();
    });
    $BoxtService.getGameState(gameId, function (success) {
        $scope.state = success;
        success.Board.Items.forEach(function (item) {
            $scope.board.Items.push(item);
        });
        $BoxtService.hub.server.joinGame(success.Game.Id);
    }, function (error) {

    });
    $scope.sendUpdate = function (index) {
        $BoxtService.hub.server.updateGame(gameId,
            {
                turn: {
                    index: index,
                }
            });
    }
}
