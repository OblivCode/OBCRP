from PyQt6.QtCore import *
from PyQt6.QtGui import *
from PyQt6.QtWidgets import *
from queue import Queue
from windows import MainWindow
from modules import services
import sys, websockets, asyncio, threading, time, signal

services_list = [
    'YouTube', 'GeForce Now', 'Twitch', 'SoloLearn'
]

def UI():
    app = QApplication(sys.argv)

    window = MainWindow()
    window.load(services_list)

    #queues
    jsonQ = Queue(1) #queue for data retrieved from extension
    checkedQ = Queue(1) #queue for list containing checked services
    
    #threads
    listener_thread = threading.Thread(target=Listener, name='Listener Thread', args=(jsonQ,))
    check_thread = threading.Thread(target=Check, name='Check Thread', args=(checkedQ, window.get_checked))
    services_thread = threading.Thread(target=services.MainHandler, name='Services Thread', args=(jsonQ, checkedQ))
    
    listener_thread.start()
    check_thread.start()
    services_thread.start()
    
    sys.exit(app.exec())
    

def Listener(jsonQ: Queue):
    asyncio.set_event_loop( asyncio.new_event_loop())

    HOST = 'localhost'
    PORT = 800

    async def handler(websocket, path):
        data = await websocket.recv()
        if jsonQ.full():
            jsonQ.get_nowait()
        jsonQ.put_nowait(data)
        

    #start ws server
    async def ws():
        try:
            async with websockets.serve(handler, HOST, PORT):
                await asyncio.Future()
        except:
            await ws()
    asyncio.get_event_loop().run_until_complete(ws())

def Check(checkedQ: Queue, check_func):
    while True:
        time.sleep(0.5)
        checked_l = check_func()
        if checkedQ.full():
            checkedQ.get_nowait()
        checkedQ.put_nowait(checked_l)


#events
def on_exit():
    print('app closed')



signal.signal(signal.SIGTERM, on_exit)

UI()
    



