var connected = false
var ws
function start() {
    if(connected) {return}
    ws = new WebSocket("ws://localhost:800")
    
        connected = true
        ws.onopen = function() {
            console.log('connected');
            connected = true
            Update()
        }
        ws.onmessage = function(message) {
            console.log(message)
            if(message == "resend") {
                Update()
            }
        }
        ws.onclose = function() {
            connected = false
        }
}

chrome.windows.onRemoved.addListener(function(windowid) {
    if(connected) {
        ws.send("closed")
        ws.close(1, "window closed")
    }
})

chrome.tabs.onActivated.addListener(function(activeInfo) {
    Update()
})
chrome.tabs.onUpdated.addListener(function(tabId, changeInfo, tab) {
    if(connected) {
        let bundle = {
            "title": tab.title,
            "url": tab.url
        }
        let jsonBundle = JSON.stringify(bundle)
        console.log(jsonBundle)
        ws.send(jsonBundle)
    }
})

function Update() {
    chrome.tabs.query({ currentWindow: true, active: true }, function (tabs) {
        let tab = tabs[0]
        if(connected) {
            let bundle = {
                "title": tab.title,
                "url": tab.url
            }
            let jsonBundle = JSON.stringify(bundle)
            console.log(jsonBundle)
            ws.send(jsonBundle)
        }
    });
}


setInterval(start, 1000)