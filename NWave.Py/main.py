# This is a sample Python script.
import os
import threading

# Press Shift+F10 to execute it or replace it with your code.
# Press Double Shift to search everywhere for classes, files, tool windows, actions, and settings.

import PySimpleGUI
import PySimpleGUI as sg

import requests as rq
import sys
import types
import httpx
import asyncio
import concurrent.futures
import time

# print(f'Connecting to {SERVER_ENDPOINT}...')

c = httpx.AsyncClient()
from urllib3.exceptions import InsecureRequestWarning

# Suppress only the single warning from urllib3 needed.

rq.packages.urllib3.disable_warnings(category=InsecureRequestWarning)

tp = concurrent.futures.ThreadPoolExecutor(max_workers=3)

K_OUTPUT = '-OUTPUT-'
K_OUTPUT2 = '-OUTPUT2-'

K_IN = 'in'
K_LB = 'lb'
K_VOL = 'vol'

K_ADDYT = 'b_AddYT'
K_UPDATE = 'b_Update'
K_STOP = 'b_Stop'
K_PAUSE = 'b_Pause'
K_PLAY = 'b_Play'
K_ADD = 'b_Add'
K_LIST = 'b_List'

g_sounds = []
window: sg.Window = None


def req(method, url, data=None, hdr=None):
    url2 = f'http://{HOST}:{PORT}/{url}'
    print(url2)
    re = rq.request(method, url2, data=data, headers=hdr, timeout=None, verify=False)
    # print(re)
    output = re.content.decode('utf-8').strip()
    return output


HOST_LOCAL1 = "192.168.1.79"
HOST_LOCAL2 = "localhost"
HOST_REMOTE = "208.110.232.218"
HOST = None
PORT = '60900'

if list(sys.argv).count('-d') > 0:
    print('debug')
    HOST = HOST_LOCAL2
else:
    print('remote')
    HOST = HOST_REMOTE


def sound_play(b):
    return req('POST', 'Play', data=b)


def sound_stop(b):
    return req('POST', f'Stop', data=b)


def sound_pause(b):
    return req('POST', f'Pause', data=b)


def format_values(values, k=K_LB):
    lb_ = values[k]
    b = ','.join(lb_)
    return b


def periodic_task():
    while True:
        # Perform the desired action
        re = req('GET', f'Status')
        if re:
            # print(re)
            try:
                window[K_OUTPUT2].update(re)
            except:
                pass
        # Adjust the sleep duration based on your specific interval
        time.sleep(1.5)


# Create a thread to run the periodic_task function
timer_thread = threading.Thread(target=periodic_task)

# Set the thread as a daemon so it terminates when the main program ends
timer_thread.daemon = True


# region

def sound_add(b):
    re = req('POST', 'Add', data=b)
    return re


def sound_add_youtube(b):
    re = req('POST', 'AddYouTube', data=b)
    return re


def sound_list():
    re = req('GET', 'List')
    rg = re.split('\n')
    return rg


def ui_update(x):
    ui_update_output(x)
    ui_update_sounds()


def ui_update_output(x):
    window[K_OUTPUT].update(x.result())
    time.sleep(3)
    window[K_OUTPUT].update('...')


def ui_update_sounds():
    global g_sounds
    global window
    g_sounds = sound_list()
    if window:
        window[K_LB].update(g_sounds)


def sound_update(x, h):
    re = req('POST', 'Update', data=x, hdr=h)
    rg = re.split('\n')
    return rg


# endregion

def main():
    global window
    global g_sounds
    global timer_thread

    ui_update_sounds()

    listbox = sg.Listbox(values=g_sounds, size=(50, 20), key=K_LB, select_mode=sg.SELECT_MODE_MULTIPLE,
                         enable_events=True)

    sg.theme("dark grey 8")

    layout = [
        # map(lambda x: sg.Button(x), rg),
        [listbox],
        [sg.InputText(key=K_IN, size=(50, 1)), sg.InputText(key=K_VOL, size=(5, 1))],
        [sg.Text(f"...", key=K_OUTPUT)],
        [sg.Text(f"...", key=K_OUTPUT2)],
        [
            sg.Button("Play", key=K_PLAY), sg.Button("Pause", key=K_PAUSE), sg.Button("Stop", key=K_STOP),
            sg.Button("Add", key=K_ADD), sg.Button("Add YT", key=K_ADDYT),
            sg.Button("List", key=K_LIST), sg.Button("Update", key=K_UPDATE)
        ]
    ]

    window = sg.Window('NWave', layout)

    while True:

        f1, f2, f3 = None, None, None

        event, values = window.read()

        print((event, values))
        if event == sg.WIN_CLOSED or event == 'Ok':
            break

        output = None
        print(f'event: {event} | values: {values}')

        if event == K_PLAY:
            f1 = tp.submit(sound_play, format_values(values))
            f1.add_done_callback(ui_update_output)

        if event == K_STOP:
            f2 = tp.submit(sound_stop, format_values(values))
            f2.add_done_callback(ui_update_output)

        if event == K_PAUSE:
            f2 = tp.submit(sound_pause, format_values(values))
            f2.add_done_callback(ui_update_output)

        if event == K_ADD:
            f3 = tp.submit(sound_add, values[K_IN])
            f3.add_done_callback(ui_update)

        if event == K_ADDYT:
            f3 = tp.submit(sound_add_youtube, values[K_IN])
            f3.add_done_callback(ui_update)

        if event == K_LIST:
            f3 = tp.submit(ui_update_sounds)
            f3.add_done_callback(ui_update_output)

        if event == K_UPDATE:
            f3 = tp.submit(sound_update, format_values(values), {'Vol': values[K_VOL]})
            f3.add_done_callback(ui_update_output)

        # if f1 and f1.done():
        #     output2 = f1.result()
        # if f2 and f2.done():
        #     output2 = f2.result()

        if output:
            window[K_OUTPUT].update(output)
        if not timer_thread.is_alive():
            timer_thread.start()

    window.close()
    return


if __name__ == '__main__':
    main()
