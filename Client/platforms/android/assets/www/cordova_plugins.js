cordova.define('cordova/plugin_list', function(require, exports, module) {
module.exports = [
    {
        "id": "cordova-plugin-statusbar.statusbar",
        "file": "plugins/cordova-plugin-statusbar/www/statusbar.js",
        "pluginId": "cordova-plugin-statusbar",
        "clobbers": [
            "window.StatusBar"
        ]
    },
    {
        "id": "cordova-plugin-facebook4.FacebookConnectPlugin",
        "file": "plugins/cordova-plugin-facebook4/www/facebook-native.js",
        "pluginId": "cordova-plugin-facebook4",
        "clobbers": [
            "facebookConnectPlugin"
        ]
    },
    {
        "id": "cordova-plugin-x-socialsharing.SocialSharing",
        "file": "plugins/cordova-plugin-x-socialsharing/www/SocialSharing.js",
        "pluginId": "cordova-plugin-x-socialsharing",
        "clobbers": [
            "window.plugins.socialsharing"
        ]
    },
    {
        "id": "cordova-universal-links-plugin.universalLinks",
        "file": "plugins/cordova-universal-links-plugin/www/universal_links.js",
        "pluginId": "cordova-universal-links-plugin",
        "clobbers": [
            "universalLinks"
        ]
    }
];
module.exports.metadata = 
// TOP OF METADATA
{
    "cordova-plugin-statusbar": "2.2.0",
    "cordova-plugin-whitelist": "1.3.0",
    "cordova-plugin-facebook4": "1.7.4",
    "cordova-plugin-x-socialsharing": "5.1.3",
    "cordova-universal-links-plugin": "1.2.0"
};
// BOTTOM OF METADATA
});