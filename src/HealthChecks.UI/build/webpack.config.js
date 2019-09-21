const webpack = require('webpack');
const path = require('path');

module.exports = {
    devtool: 'none',
    entry: path.resolve(__dirname, "../client/index.tsx"),
    resolve: {
        extensions: [".tsx", ".ts", ".js", ".json"]
    },
    output: {
        path: path.join(__dirname, "../assets"),
        filename: 'healthchecks-bundle.js'
    },
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                loader: "awesome-typescript-loader",
                exclude: [/(node_modules)/]
            },
            {
                loader: 'url-loader',
                test: /\.(png|jpg|gif|svg)$/
            },
            {
                test: /\.css$/i,
                use: ['style-loader', 'css-loader'],
            }
        ]
    },
    plugins: [
        new webpack.DllReferencePlugin({
            context: ".",
            manifest: require("../assets/vendors-manifest.json")
        })
    ]
};