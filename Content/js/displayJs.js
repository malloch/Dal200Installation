



var DataCollectedFromController = [];


var datasetOfContent = ["HTML/na_1.html","HTML/na_2.html","HTML/na_3.html","HTML/na_4.html"];

/*
   //sum of values in a row
    d3.csv("datafordisplay.csv", function (ignData) {
        dataset = ignData;

    })
*/

//get data from textbox and display appropriate text
    function shower(){

        //d3.select("body");

//the div holding th e image and text
        /*var divWithContent= d3.select("body").append("div")
            .attr("id","firstDiv")
            .style("width",widthOfContentDiv)
            .style("height",heightOfContentDiv)
            .style("background-color", "red");
            divWithContent.append("img")
                .attr("src", "images/first.jpg")
                .style("width",widthOfContentDiv);
            divWithContent.append("p")
                .style("color", "white").text("Tester Here!");

            divWithContent.style("position","absolute")
            .transition().duration(500).style("left","200px").style("top","300px");*/

           
     }




     function selectContent(){

        var ddlSelectedPosition = d3.select("#ddlPosition").property('value');

        d3.select('#ifmContent').data(datasetOfContent).attr('src',datasetOfContent[ddlSelectedPosition]);

     }









function WebSocketTest()
         {
            if ("WebSocket" in window)
            {
               alert("WebSocket is supported by your Browser!");
               
               // Let us open a web socket
               var ws = new WebSocket("ws://134.190.155.223/Dal200");
				
               ws.onopen = function()
               {
                  // Web Socket is connected, send data using send()
                  ws.send("Message to send");
                  alert("Message is sent...");
               };
				
               ws.onmessage = function (evt) 
               { 
                  var received_msg = evt.data;
		
			var theRow = JSON.parse(received_msg);
		    console.log(theRow["trackerData"][0].position);

                //d3.select("#firstDiv").transition().duration(500)
        		//.style("left", theRow["trackerData"][0].position.x +"px").style("top",theRow["trackerData"][0].position.y +"px");
    
                  
                  //alert("Message is received...");
               };
				
               ws.onclose = function()
               { 
                  // websocket is closed.
                  alert("Connection is closed..."); 
               };
					
               window.onbeforeunload = function(event) {
                  socket.close();
               };
            }
            
            else
            {
               // The browser doesn't support WebSocket
               alert("WebSocket NOT supported by your Browser!");
            }
         }

        // WebSocketTest();






function moveDiv(){

    var ddlSelectedPosition = d3.select("#ddlPosition").property('value');

    d3.select("#firstDiv").transition().duration(500)
        .style("left", wallSections[ddlSelectedPosition][0]).style("top",wallSections[ddlSelectedPosition][1]);
    

}


function moveDivDDT(){

    var ddlSelectedPosition = d3.select("#ddlPosition").property('value');


   console.log(DataCollectedFromController);

console.log(DataCollectedFromController[0].trackerData);

    d3.select("#firstDiv").transition().duration(500)
        .style("left", DataCollectedFromController[0].trackerData.position.x).style("top",DataCollectedFromController[0].trackerData.position.y);
    


}

