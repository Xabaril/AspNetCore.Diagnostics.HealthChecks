const webpack = require('webpack');
const path = require('path');

module.exports = {
    mode: "development",
    devtool: "hidden-source-map",
    entry: path.resolve(__dirname, "../client/index.tsx"),
    output: {
        path: path.join(__dirname, "../assets"),
        filename: 'healthchecks-bundle.js'
    },
    resolve: {
        // Add `.ts` and `.tsx` as a resolvable extension.
        extensions: [".tsx", ".ts", ".js", ".json"],
        // Add support for TypeScripts fully qualified ESM imports.
        extensionAlias: {
            ".js": [".js", ".ts"],
            ".cjs": [".cjs", ".cts"],
            ".mjs": [".mjs", ".mts"]
        }
    },
    module: {
        rules: [
            // all files with a `.ts`, `.cts`, `.mts` or `.tsx` extension will be handled by `ts-loader`
            { test: /\.tsx?$/, loader: "ts-loader" },
            {
                loader: 'url-loader',
                test: /\.(png|jpg|gif|svg|woff2)$/
            },
            {
                test: /\.css$/i,
                use: ['style-loader', 'css-loader'],
            }
        ]
    }
};