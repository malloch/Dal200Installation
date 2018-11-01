"use strict"

window.onload = init;

var data = [];
var socket = null;
var connected = false;
var connectionInterval = 60000;
var screensaver = 0;
var screensaver_timeout = 30000;
var contentIdx = -1;
var anecdoteRepeater = null;
var files = ["lovelace",    "lamarr",       "hopper",       "keller",
             "hamilton",    "wss",          "suresh",       "wv",
             "watters",     "jutla",        "bahr-gedalia", "zincir-heywood",
             "alshazly",    "perry",        "orji",         "worsley",
             "tu",          "klawe",        "payette",      "condon",
             "murphy",      "reid",         "xiao",         "cannon" ];

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
                d3.select('#ifmContent').attr('src', "");
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
            // cancel anecdote scroller if necessary
            if (anecdoteRepeater != null)
                clearInterval(anecdoteRepeater);
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

function showCode() {
    screensaver = 0;
    let videoSrc = d3.select('#videoSrc').attr('src');
    let video = document.getElementById('video');
    if (videoSrc != "") {
        if (!video.ended) {
            video.pause();
        }
    }
    console.log("showing code");
    // cancel anecdote scroller if necessary
    if (anecdoteRepeater != null)
        clearInterval(anecdoteRepeater);
    // hide existing content
    d3.select('#name').text("");
    d3.select('#lifespan').text("");
    d3.select('#anecdote').text("");
    d3.select('#photo').attr('src', '');
    d3.select('#videoSrc').attr('src', '');
    video.load();

    // load code renderer
    d3.select('#ifmContent')
      .attr('src', "../Software/CodeView/CodeView.html");
}

function showOverView() {
    screensaver = 0;
    let videoSrc = d3.select('#videoSrc').attr('src');
    let video = document.getElementById('video');
    if (videoSrc != "") {
        if (!video.ended) {
            video.stop();
        }
    }
    console.log("showing code");
        // cancel anecdote scroller if necessary
    if (anecdoteRepeater != null)
        clearInterval(anecdoteRepeater);
        // hide existing content
    d3.select('#name').text("");
    d3.select('#lifespan').text("");
    d3.select('#anecdote').text("");
    d3.select('#photo').attr('src', '');
    d3.select('#videoSrc').attr('src', '');
    video.load();

    // load overview renderer
    d3.select('#ifmContent')
      .attr('src', "../Software/OverView/OverView.html");
}

function chooseIndex(idx) {
    let keys = Object.keys(data);
    let entry = data[keys[idx]];
    console.log("Loading content["+contentIdx+"] :", keys[contentIdx]);

    // cancel anecdote scroller if necessary
    if (anecdoteRepeater != null) {
        clearInterval(anecdoteRepeater);
        d3.select('#anecdote').transition()
                                  .duration(1000)
                                  .style("opacity", 0);
    }

    let name = null;
    if (entry.displayName) {
        name = entry.displayName;
        name = name.split("\n");
    }
    else if (entry.name) {
        name = entry.name;
        name = name.split(" ");
    }
    $('#name').empty();
    if (name) {
        for (var i in name) {
            $('#name').append("<div class='namediv'><span>"+name[i]+"</span></div>");
        }
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

    function loadAnecdote() {
        let numAnecdotes = entry.anecdotes.length;
        let idx = entry.anecdotes.counter;
        let obj = d3.select('#anecdote');
        obj.transition()
               .duration(1000)
               .style("opacity", 0);
        obj.transition()
               .delay(1000)
               .text(entry.anecdotes[idx].body)
               .duration(1000)
               .style("opacity", 1.0);
        idx += 1;
        if (idx >= numAnecdotes)
            idx = 0;
        entry.anecdotes.counter = idx;
    }

    if (!entry.video && entry.anecdotes) {
        loadAnecdote();
        anecdoteRepeater = setInterval(function() {
            loadAnecdote();
        }, 15000);
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
    // dismiss iframe if necessary
    d3.select('#ifmContent').attr('src', "");
    screensaver = 0;
    switch (e.which) {
        case 78:
            /* 'N' */
            // load the next entry
            let keys = Object.keys(data);
            contentIdx++;
            if (contentIdx > (keys.length -1))
                contentIdx = 0;
            chooseIndex(contentIdx);
            break;
        case 67:
            // 'C'
            showCode();
            break;
        case 79:
            // 'O'
            showOverView();
            break;
        default:
            console.log("keypress:", e.which);
    }
})
