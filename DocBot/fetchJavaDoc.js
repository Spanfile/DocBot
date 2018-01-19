var page = require('webpage').create();
system = require('system');

page.onLoadFinished = function () {
    console.log(system.args[3]);
    page.evaluate(function() {
        document.getElementById('search').value = system.args[3];
    });
    setTimeout(function() {
        console.log(page.content);
        phantom.exit();
    }, 500);
};

page.settings.userAgent = system.args[2];
page.open(system.args[1]);
