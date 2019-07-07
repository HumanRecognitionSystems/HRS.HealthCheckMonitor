var gulp = require("gulp"),
    merge = require("merge-stream"),
    rimraf = require("gulp-rimraf");

var deps = {
    "jquery": { "dirs": { "dist": "" } },
    "bootstrap": { "dirs": { "dist": "" } },
    "bootstrap-table": { "dirs": { "dist": "" } }
};

gulp.task("copyLibs", function () {
    var streams = [];

    for (var prop in deps) {
        console.log("Prepping for libs:" + prop);

        var source = "node_modules/" + prop + "/";
        var dest = "wwwroot/lib/";
        if (deps[prop].name != null) {
            dest += deps[prop].name + "/";
        } else {
            dest += prop + "/";
        }

        console.log("Source:" + source + " Dest:" + dest);

        if (deps[prop].dirs != null) {
            for (var sub in deps[prop].dirs) {
                console.log("  Will Copy:" + source + sub + "/**/* to " + dest + deps[prop].dirs[sub]);
                streams.push(
                    gulp.src(source + sub + "/**/*")
                        .pipe(gulp.dest(dest + deps[prop].dirs[sub]))
                );
            }
        }
    }

    return merge(streams);
});

gulp.task("cleanLibs", function () {
    return gulp.src(["wwwroot/lib/*"])
        .pipe(rimraf());
});