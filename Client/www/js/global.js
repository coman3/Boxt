var backButtonAction = function (e) {

}
var auth;
var lobbyHub;
var userInfo = {};
var hostAddress = "http://192.168.0.17/";
$.connection.hub.url = hostAddress + "signalr";

var app = angular.module('Boxt', ["ngMaterial", "ngRoute"]);

app.config(function ($mdThemingProvider, $routeProvider) {
    $mdThemingProvider.theme('default')
        .primaryPalette('green')
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
        .when("/login", {
            templateUrl: "partials/login.html",
            controller: "Login"
        })
        .when("/games/TicTacToe", {
            templateUrl: "partials/games/TicTacToe.html",
            controller: "TicTacToe"
        })
});

