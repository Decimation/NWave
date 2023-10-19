# This is a sample Python script.
import os
import threading

import PySimpleGUI
# Press Shift+F10 to execute it or replace it with your code.
# Press Double Shift to search everywhere for classes, files, tool windows, actions, and settings.

import PySimpleGUI as sg;

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
output_ = '-OUTPUT-'
output_2 = '-OUTPUT2-'

window: PySimpleGUI.Window = None


def req(method, url, data=None):
    url2 = f'http://{HOST}:{PORT}/{url}'
    print(url2)
    re = rq.request(method, url2, data=data)
    print(re)
    output = re.content.decode('utf-8').strip()
    return output


# HOST = "192.168.1.79"
HOST = "208.110.232.218"
PORT = '60900'


def play(b):
    return req('POST', 'Play', data=b)


def stop(b):
    return req('POST', f'Stop', data=b)


def get_snds(values):
    lb_ = values['lb']
    b = ','.join(lb_)
    return b


def periodic_task():
    while True:
        # Perform the desired action
        re = req('GET', f'Status')
        if re:
            print(re)
            window[output_2].update(re)
        # Adjust the sleep duration based on your specific interval
        time.sleep(1.5)


# Create a thread to run the periodic_task function
timer_thread = threading.Thread(target=periodic_task)

# Set the thread as a daemon so it terminates when the main program ends
timer_thread.daemon = True


# Start the thread


def sounds():
    re = req('GET', 'List')
    rg = re.split('\r\n')
    return rg


g_sounds = sounds()


def update_output1(x):
    window[output_].update(x.result())
    time.sleep(3)
    window[output_].update('...')


def main():
    global window
    global g_sounds
    global timer_thread

    listbox = sg.Listbox(values=g_sounds, size=(50, 20), key='lb', select_mode=sg.SELECT_MODE_MULTIPLE,
                         enable_events=True)
    layout = [
        # map(lambda x: sg.Button(x), rg),
        [listbox],
        [sg.Text(f"...", key=output_)],
        [sg.Text(f"...", key=output_2)],
        [sg.Button("Play", key='b_Play'), sg.Button("Stop", key='b_Stop')]
    ]
    window = sg.Window('PiCore', layout)
    timer_thread.start()

    while True:

        f1, f2, f3 = None, None, None

        event, values = window.read()

        if event == sg.WIN_CLOSED or event == 'Ok':
            break

        output = None
        print(f'event: {event} | values: {values}')

        if event == 'b_Play':
            # lb_ = values['lb'][0][1]
            f1 = tp.submit(play, get_snds(values))
            f1.add_done_callback(update_output1)

        if event == 'b_Stop':
            # lb_ = values['lb'][0][1]
            f2 = tp.submit(stop, get_snds(values))
            f2.add_done_callback(update_output1)

        # if f1 and f1.done():
        #     output2 = f1.result()
        # if f2 and f2.done():
        #     output2 = f2.result()

        if output:
            window[output_].update(output)

    window.close()
    return


if __name__ == '__main__':
    main()
