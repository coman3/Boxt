var app = angular.module('myApp', []);
app.controller("OAuthCtrl", function ($scope) {
    $scope.facebookLogin = function () {

        facebookConnectPlugin.login(["email"], function (success) {
            console.log("Success!")
            console.log("Status: " + success.status + "\n" +
            "Access Token: " + success.authResponse.accessToken);
            alert(success);
        }, function (error) {
            console.log("Error!")
            alert(error);
        });
    }
})