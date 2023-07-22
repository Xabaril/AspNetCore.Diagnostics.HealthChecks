var path = require("path");
var webpack = require("webpack");

module.exports = {
    entry: {
        vendors: [path.join(__dirname, "vendors.js")]
    },
    output: {
        path: path.join(__dirname, "../assets"),
        filename: "[name]-dll.js",
        library: "[name]"
    },
    plugins: [
        new webpack.DllPlugin({
            path: path.join(__dirname, "../assets", "[name]-manifest.json"),
            name: "[name]"
        })
    ]
};
