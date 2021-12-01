import {build} from "esbuild";
import glob from "glob";

build({
    entryPoints: glob.sync("./src/**/*.ts"),
    outbase: "./src",
    outdir: "./build",
    platform: "browser"
});