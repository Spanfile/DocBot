var page = require('webpage').create();
system = require('system');

page.onLoadFinished = function () {
    setTimeout(function() {
        console.log(page.content);
        phantom.exit();
    }, 1000);
};

page.open(system.args[1]);
