"use strict"

window.onload = init;

var socket = null;
var connected = false;
var connectionInterval = 60000;
var screensaver = 0;
var screensaver_timeout = 30000;
var DataCollectedFromController = [];
var contentID = 24;
var datasetOfContent =[[0, "HTML/in_1.html"],
                       [1, "HTML/in_2.html"],
                       [2, "HTML/in_3.html"],
                       [3, "HTML/in_4.html"],
                       [4, "HTML/in_5.html"],
                       [5, "HTML/in_6.html"],
                       [6, "HTML/in_7.html"],
                       [7, "HTML/in_8.html"],
                       [8, "HTML/lo_1.html"],
                       [9, "HTML/lo_2.html"],
                       [10, "HTML/lo_3.html"],
                       [11, "HTML/lo_4.html"],
                       [12, "HTML/lo_5.html"],
                       [13, "HTML/lo_6.html"],
                       [14, "HTML/lo_7.html"],
                       [15, "HTML/na_1.html"],
                       [16, "HTML/na_2.html"],
                       [17, "HTML/na_3.html"],
                       [18, "HTML/na_4.html"],
                       [19, "HTML/na_5.html"],
                       [20, "HTML/na_6.html"],
                       [21, "HTML/na_7.html"],
                       [22, "HTML/na_8.html"],
                       [23, "HTML/na_9.html"],
                       [24, "../Software/SplashView/SplashView.html"]];

function init() {
    console.log('init!');

    function makeDDL() {
        var varDDLPosition = document.getElementById("ddlPosition");
        if (varDDLPosition) {
            for (var i = 0; i < datasetOfContent.length+1; ++i) {
                var optn = document.createElement("OPTION");
                optn.text = i;
                optn.value = i;
                ddlPosition.options.add(optn);
            }
        }
    }
    makeDDL();

    function selectContent(id) {
        if (id != contentID) {
            console.log("selecting id", id);
            d3.select('#ifmContent').data(datasetOfContent)
                                    .attr('src', datasetOfContent[id][1]);
            contentID = id;
        }
        else {
            console.log("id", id, "already selected");
        }
    }

    if (navigator.onLine) {
        console.log("You are Online");
    }
    else {
        console.log("You are Offline");
    }

    function openWebSocket() {
        console.log("   Trying to connect...");

        // Create a new WebSocket.
        socket = new WebSocket("ws://192.168.1.120/Dal200");

        socket.onopen = function(event) {
            console.log("Connection established");
            connected = true;
        }

        socket.onclose = function(event) {
            console.log("Connection dropped, will try to reconnect within",
                        connectionInterval/1000, "seconds");
            connected = false;
        }

        socket.onmessage = function(event) {
            var data = null;
            if (event.data)
                data = JSON.parse(event.data);
            if (!data)
                return;
            if (data.dwellIndex != null) {
                selectContent(data.dwellIndex);
            }
            if (data.trackerData) {
                screensaver = 0;
            }
        }
    }

    // open webSocket
    openWebSocket();

    // check the websocket periodically
    setInterval(function() {
        console.log("checking websocket...");
        if (connected == true) {
            console.log("   Socket ok.");
            return;
        }
        openWebSocket();
    }, connectionInterval);

    // activate the screensaver if necessary
    setInterval(function() {
        console.log("checking for activity...");
        if (1 == screensaver) {
            // contentID = Math.floor(Math.random() * 24);
            // console.log("  switching page to", contentID);
            console.log("starting screensaver");
            d3.select('#ifmContent').data(datasetOfContent)
                                    .attr('src', datasetOfContent[24][1]);
        }
        ++screensaver;
    }, screensaver_timeout);

    window.onbeforeunload = function(event) {
        socket.close();
    };
}

// Enable launching pages by pressing the 'N' key
$('body').on('keydown.list', function(e) {
    switch (e.which) {
        case 78:
            /* 'N' */
            contentID++;
            if (contentID > (datasetOfContent.length -1))
              contentID = 0;
            console.log("Loading page", contentID);
            d3.select('#ifmContent').data(datasetOfContent)
                                    .attr('src', datasetOfContent[contentID][1]);
            break;
        default:
          console.log("keypress:", e.which);
    }
})
