var backButtonAction = function (e) {

}
var auth;
var lobbyHub;
var userInfo = {};
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
        .when("/menu/friends", {
            templateUrl: "partials/menu/friends.html",
        })
        .when("/games/TicTacToe", {
            templateUrl: "partials/games/TicTacToe.html",
            controller: "TicTacToe"
        })
});

