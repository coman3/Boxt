app.service("BoxtNotifyService", function ($rootScope, $http, $mdToast, BoxtGameService) {
    this.OnServerMessage = function (message) {
        if (message.Event != null) {
            if (message.Event.Event != null && message.Event.Event.CreateInvite != null) {
                var invite = message.Event.Event.CreateInvite;
                BoxtGameService.OnCreateInvite(invite);
                var toast = $mdToast.simple()
                    .textContent(invite.Inviter.Name + " has invited you to play " + invite.Game.GameApplication.Name)
                    .action('Play Now')
                    .highlightAction(true)

                $mdToast.show(toast).then(function (response) {
                    if (response == 'ok') {
                        $rootScope.BoxtService.navigate("games/" + invite.Game.GameApplication.Id + "/" + invite.Game.Id);
                    }
                });
                return true;
            }
            if (message.Event.Event != null && message.Event.Event.AcceptInvite != null) {
                var invite = message.Event.Event.AcceptInvite;
                BoxtGameService.OnAcceptInvite(invite);
                var toast = $mdToast.simple()
                    .textContent(invite.Inviter.Name + " has accepted your invite to play " + invite.Game.GameApplication.Name)
                    .action('Play Now')
                    .highlightAction(true)

                $mdToast.show(toast).then(function (response) {
                    if (response == 'ok') {
                        $rootScope.BoxtService.navigate("games/" + invite.Game.GameApplication.Id + "/" + invite.Game.Id);
                    }
                });
                return true;
            }
        }
        return false;
    }

});