var path = require("path");
var webpack = require("webpack");

module.exports = {
    entry: {
        vendors: [path.join(__dirname, "vendors.js")]
    },
    output: {
        path: path.join(__dirname, "../Assets"),
        filename: "[name]-dll.js",
        library: "[name]"
    },
    plugins: [
        new webpack.DllPlugin({
            path: path.join(__dirname, "../Assets", "[name]-manifest.json"),
            name: "[name]"            
        }),
        new webpack.optimize.OccurrenceOrderPlugin()        
    ]    
};
