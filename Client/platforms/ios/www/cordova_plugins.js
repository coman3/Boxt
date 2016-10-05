cordova.define('cordova/plugin_list', function(require, exports, module) {
module.exports = [
    {
        "id": "phonegap-facebook-plugin.FacebookConnectPlugin",
        "file": "plugins/phonegap-facebook-plugin/facebookConnectPlugin.js",
        "pluginId": "phonegap-facebook-plugin",
        "clobbers": [
            "facebookConnectPlugin"
        ]
    },
    {
        "id": "cordova-plugin-statusbar.statusbar",
        "file": "plugins/cordova-plugin-statusbar/www/statusbar.js",
        "pluginId": "cordova-plugin-statusbar",
        "clobbers": [
            "window.StatusBar"
        ]
    }
];
module.exports.metadata = 
// TOP OF METADATA
{
    "phonegap-facebook-plugin": "0.12.0",
    "cordova-plugin-statusbar": "2.2.0"
};
// BOTTOM OF METADATA
});