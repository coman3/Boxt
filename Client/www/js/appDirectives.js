app.directive('gameGridList', function () {
    return {
        templateUrl: 'controls/gameGridList.tmpl.html',
        scope: {
            games: '=',
            cols: '@',
            rowHeight: '@',
            itemClick: '='
        }
    }
});
app.directive('sectionHeader', function () {
    return {
        templateUrl: 'controls/sectionHeader.tmpl.html',
        transclude: true,
        scope: {
            header: "@"
        }
    }
});
app.directive('friendSelector', ["BoxtService", function ($BoxtService) {
    return {
        templateUrl: 'controls/friendSelector.tmpl.html',
        controller: "menu/friends",
        scope: {
            headerMessage: "@",
            onFriendsSelected: "=",
        }
    }
}]);