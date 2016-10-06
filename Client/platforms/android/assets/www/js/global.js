var backButtonAction = function (e) {

}

var app = angular.module('TextIt', ["ngMaterial", "ngRoute"]);
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
});

