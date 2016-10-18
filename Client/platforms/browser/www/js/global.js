var backButtonAction = function (e) {
    e.preventDefault();
};

document.addEventListener("backbutton", function (e) {
    backButtonAction();
}, false);

var hostAddress = "http://192.168.0.17:1234/";
$.connection.hub.url = hostAddress + "signalr";

var app = angular.module('Boxt', ["ngMaterial", "ngRoute"]);

app.config(function ($mdThemingProvider, $routeProvider) {
    $mdThemingProvider.theme('default')
        .primaryPalette('blue')
        .accentPalette('orange');
    $routeProvider
        .when("/", {
            templateUrl: "partials/load.html",
            controller: "Loader"
        })
        .when("/lobby", {
            templateUrl: "partials/lobby.html",
            controller: "Lobby"
        })
        .when("/games/:gameAppId/:gameId", {
            templateUrl: function (urlattr) {
                return "Games/" + urlattr.gameAppId + ".html";
            },
            controller: function ($scope, $routeParams, $controller) {
                $controller("GameController_" + $routeParams.gameAppId, {
                    $scope: $scope
                });
            }
        })
        .when("/menu/create/:gameType", {
            templateUrl: "partials/menu/create.html",
            controller: "menu/create"
        })
        .when("/menu/invite/:inviteId", {
            templateUrl: "partials/menu/invite.html",
            controller: "menu/invite"
        })
        .when("/menu/games/:filter", {
            templateUrl: "partials/menu/games.html",
            controller: "menu/games"
        })
        .when("/menu/friends/:friend", {
            templateUrl: "partials/menu/friends.html",
            controller: "menu/friends"
        })
        .otherwise({
            redirectTo: "/"
        });
});

