from pypresence import Presence
import time, psutil

def IsDiscordRunning() -> bool:
    for proc in psutil.process_iter():
        try:
            if 'discord' in proc.name().lower():
                return True
        except:
            print('Print psutil error')
    return False

if not IsDiscordRunning():
    print('Discord is not running. Waiting.')
    while not IsDiscordRunning():
        time.sleep(1)
    time.sleep(5)

#
RPC = Presence('750690488809422933')
RPC.connect()

def SetPresence(state: str, details: str, service:str, timer: bool = False):
    if timer:
        start_time = time.time()
        RPC.update(start=start_time, state=state, details=details, large_image=service, large_text='OBC\'s RP: https://github.com/OblivCode/OBCRP')
    else:
        RPC.update(state=state, details=details, large_image=service, large_text='OBC\'s RP: https://github.com/OblivCode/OBCRP')

def ClearPresence():
    RPC.clear()

def Close():
    RPC.close()