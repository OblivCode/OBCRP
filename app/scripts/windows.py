from queue import Queue
from PyQt6.QtGui import QAction
from PyQt6.QtWidgets import *
from PyQt6.QtCore import *
import sys,socket

class MainWindow(QMainWindow):
    def __init__(self, *args, **kwargs):
        super(MainWindow, self).__init__(*args, **kwargs)
       
        self.setFixedSize(150,150)
        self.container = QWidget()
        self.containerLayout = QVBoxLayout()
        self.container.setLayout(self.containerLayout)
        self.widgets: list[QCheckBox] = []

        self.setCentralWidget(self.container)

    def load(self, services: list):
        #load checkboxes
        for service in services:
            item = QCheckBox(service, self)
            self.containerLayout.addWidget(item)
            self.widgets.append(item)

        
        spacer = QSpacerItem(1, 1, QSizePolicy.Policy.Minimum, QSizePolicy.Policy.Expanding)
        self.containerLayout.addItem(spacer)
        self.container.setLayout(self.containerLayout)

        #scroll
        self.scroll = QScrollArea()
        self.scroll.setVerticalScrollBarPolicy(Qt.ScrollBarPolicy.ScrollBarAlwaysOn)
        self.scroll.setHorizontalScrollBarPolicy(Qt.ScrollBarPolicy.ScrollBarAlwaysOff)
        self.scroll.setWidgetResizable(True)
        self.scroll.setWidget(self.container)
        
        self.searchbar = QLineEdit()
        self.searchbar.textChanged.connect(self.update_display)

        container = QWidget()
        containerLayout = QVBoxLayout()
        containerLayout.addWidget(self.searchbar)
        containerLayout.addWidget(self.scroll)

        container.setLayout(containerLayout)
        self.setCentralWidget(container)

        self.setGeometry(600, 100, 800, 600)


        self.show()
   
    def update_display(self, text):
        for widget in self.widgets:
            
            if text.lower() in widget.text().lower():
                widget.show()
            else:
                widget.hide()
    
    def get_checked(self) -> list[str]:
        checked_list = []
        for w in self.widgets:
            if w.isChecked():
                checked_list.append(w.text())
        return checked_list
    
    def close_window(self):
        sys.exit(0)
