"use strict"

window.onload = init;

var data = [];
var socket = null;
var connected = false;
var connectionInterval = 60000;
var screensaver = 0;
var screensaver_timeout = 5000;
var DataCollectedFromController = [];
var contentIdx = -1;
var files = ["lovelace",
             "lamarr",
             "hopper",
             "keller",
             "hamilton",
             "wss",
             "suresh",
             "wv",
             "watters",
             "jutla",
             "bahr-gedalia",
             "zincir-heywood",
             "alshazly",
             "perry",
             "orji",
             "worsley",
             "tu",
             "klawe",
             "payette",
             "condon",
             "murphy",
             "reid",
             "xiao",
             "cannon"
             ];
//                       [24, "../Software/SplashView/SplashView.html"]];

function init() {
    console.log('init!');

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
                // if (screensaver) {
                    d3.select('#ifmContent').attr('src', "");
                // }
                chooseIndex(data.dwellIndex);
            }
            screensaver = 0;
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
            let videoSrc = d3.select('#videoSrc').attr('src');
            let video = document.getElementById('video');
            if (videoSrc != "") {
                if (!video.ended) {
                    screensaver = 0;
                    return;
                }
            }
            console.log("starting screensaver");
            // hide existing content
            d3.select('#name').text("");
            d3.select('#lifespan').text("");
            d3.select('#anecdote').text("");
            d3.select('#photo').attr('src', '');
            d3.select('#videoSrc').attr('src', '');
            video.load();
            d3.select('#ifmContent')
              .attr('src', "../Software/SplashView/SplashView.html");
        }
        ++screensaver;
    }, screensaver_timeout);

    window.onbeforeunload = function(event) {
        socket.close();
    };

    // load json data
    files.forEach(function(name) {
        $.getJSON("./data/"+name+".json", function(_data) {
            let idx = files.indexOf(_data.key);
            console.log("adding "+_data.key+" at index "+idx);
            data[idx] = _data;
            data[idx].anecdotes.counter = 0;
        });
    });
}

function chooseIndex(idx) {
    let keys = Object.keys(data);
    let entry = data[keys[idx]];
    console.log("Loading content["+contentIdx+"] :", keys[contentIdx]);

    let name = null;
    if (entry.displayName) {
        name = entry.displayName;
        while (name.indexOf("\n") >= 0)
            name = name.replace("\n", "<br/>");
//        name = name.split("\n");
    }
    else if (entry.name) {
        name = entry.name;
        while (name.indexOf(" ") >= 0)
            name = name.replace(" ", "<br/>");
//        name = name.split(" ");
    }
//    console.log(name);
//    $('#name').empty();
    if (name) {
        d3.select('#name').html(name);
//        let i;
//        for (i in name) {
//            $('#name').append('<span>'+name[i]+'</span>');
//            if (i < name.length - 1)
//                $('#name').append('<br/>');
//        }
    }
    else
        d3.select('#name').text("");

    if (entry.birth) {
        if (entry.death)
            d3.select('#lifespan').text(entry.birth+"-"+entry.death);
        else
            d3.select('#lifespan').text(entry.birth+"-");
    }
    else
        d3.select('#lifespan').text("");

    if (!entry.video && entry.anecdotes) {
        let numAnecdotes = entry.anecdotes.length;
        let idx = entry.anecdotes.counter;
        d3.select('#anecdote').text(entry.anecdotes[idx].body);
        idx += 1;
        if (idx >= numAnecdotes)
            idx = 0;
        entry.anecdotes.counter = idx;
    }
    else
        d3.select('#anecdote').text("");

    let video = document.getElementById('video');
    video.pause();
    if (entry.video) {
        console.log("loading video", './images/'+entry.video);
        d3.select('#photo').attr('src', '');
        d3.select('#videoSrc').attr('src', './images/'+entry.video);
    }
    else if (entry.image) {
        console.log("loading image", './images/'+entry.image);
        d3.select('#photo').attr('src', './images/'+entry.image);
        d3.select('#videoSrc').attr('src', '');
    }
    video.load();
    if (entry.video)
        video.play();
}

// Enable launching pages by pressing the 'N' key
$('body').on('keydown.list', function(e) {
    switch (e.which) {
        case 78:
            /* 'N' */
            // dismiss screensaver if necessary
            if (screensaver) {
                d3.select('#ifmContent').attr('src', "");
                screensaver = 0;
            }
            // load a random entry
            let keys = Object.keys(data);
            contentIdx++;
            if (contentIdx > (keys.length -1))
                contentIdx = 0;
            chooseIndex(contentIdx);
            break;
        default:
            console.log("keypress:", e.which);
    }
})
