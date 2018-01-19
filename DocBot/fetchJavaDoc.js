var page = require('webpage').create();
system = require('system');

page.onLoadFinished = function () {
    page.evaluate(function(query) {
        document.getElementById('search').value = query;
    }, system.args[3]);
    setTimeout(function() {
        console.log(page.content);
        phantom.exit();
    }, 1000);
};

page.settings.userAgent = system.args[2];
page.open(system.args[1]);
