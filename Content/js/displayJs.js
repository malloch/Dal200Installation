var DataCollectedFromController = [];

var contentID = 24;

var datasetOfContent =
[
    [0,"HTML/in_1.html"],
    [1,"HTML/in_2.html"],
    [2,"HTML/in_3.html"],
    [3,"HTML/in_4.html"],
    [4,"HTML/in_5.html"],
    [5,"HTML/in_6.html"],
    [6,"HTML/in_7.html"],
    [7,"HTML/in_8.html"],

    [8,"HTML/lo_1.html"],
    [9,"HTML/lo_2.html"],
    [10,"HTML/lo_3.html"],
    [11,"HTML/lo_4.html"],
    [12,"HTML/lo_5.html"],
    [13,"HTML/lo_6.html"],
    [14,"HTML/lo_7.html"],

    [15,"HTML/na_1.html"],
    [16,"HTML/na_2.html"],
    [17,"HTML/na_3.html"],
    [18,"HTML/na_4.html"],
    [19,"HTML/na_5.html"],
    [20,"HTML/na_6.html"],
    [21,"HTML/na_7.html"],
    [22,"HTML/na_8.html"],
    [23,"HTML/na_9.html"]
];


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

function selectContent() {
    var ddlSelectedPosition = d3.select("#ddlPosition").property('value');
    d3.select('#ifmContent').data(datasetOfContent)
                            .attr('src',datasetOfContent[contentID][1]);
}

function WebSocketTest() {
    if ("WebSocket" in window) {
        //alert("WebSocket is supported by your Browser!");

        // Let us open a web socket
        var ws = new WebSocket("ws://192.168.1.120/Dal200");
				
        ws.onopen = function() {
            // Web Socket is connected, send data using send()
            //ws.send("Message to send");
            // alert("Message is sent...");
        };
				
        ws.onmessage = function(evt) {
            var received_msg = evt.data;
			var theRow = JSON.parse(received_msg);
//            var newcontentID = theRow["trackerData"][0].id;

            if (theRow.dwellIndex != null) {
                var newcontentID = theRow.dwellIndex;
                console.log(newcontentID + " " + datasetOfContent[newcontentID][1]);
                d3.select('#ifmContent').data(datasetOfContent)
                                        .attr('src', datasetOfContent[0][1]);
            }
        };
				
        ws.onclose = function() {
            // websocket is closed.
            //alert("Connection is closed...");
        };
					
        window.onbeforeunload = function(event) {
            socket.close();
        };
    }
    else {
        // The browser doesn't support WebSocket
        alert("WebSocket NOT supported by your Browser!");
    }
}
WebSocketTest();

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

function moveDiv() {
    var ddlSelectedPosition = d3.select("#ddlPosition").property('value');

    d3.select("#firstDiv").transition().duration(500)
                                       .style("left", wallSections[ddlSelectedPosition][0])
                                       .style("top", wallSections[ddlSelectedPosition][1]);
}

function moveDivDDT() {
    var ddlSelectedPosition = d3.select("#ddlPosition").property('value');

    console.log(DataCollectedFromController);

    console.log(DataCollectedFromController[0].trackerData);

    d3.select("#firstDiv")
      .transition()
      .duration(500)
      .style("left", DataCollectedFromController[0].trackerData.position.x).style("top",DataCollectedFromController[0].trackerData.position.y);
}

/*
   //sum of values in a row
    d3.csv("datafordisplay.csv", function (ignData) {
        dataset = ignData;
    })
*/

//get data from textbox and display appropriate text
function shower(){
//    d3.select("body");

//    // the div holding the image and text
//    var divWithContent = d3.select("body").append("div").attr("id", "firstDiv")
//                                                        .style("width", widthOfContentDiv)
//                                                        .style("height", heightOfContentDiv)
//                                                        .style("background-color", "red");
//    divWithContent.append("img").attr("src", "images/first.jpg")
//                                .style("width", widthOfContentDiv);
//    divWithContent.append("p").style("color", "white")
//                              .text("Tester Here!");
//
//    divWithContent.style("position", "absolute")
//                  .transition().duration(500)
//                               .style("left","200px")
//                               .style("top","300px");
}
