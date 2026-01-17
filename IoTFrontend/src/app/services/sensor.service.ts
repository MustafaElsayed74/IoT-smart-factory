import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
    providedIn: 'root'
})
export class SensorService {
    private connection: signalR.HubConnection;
    private isConnected = false;

    temperature: number = 0;
    smoke: boolean = false;
    weight: number = 0;
    productNumber: string = '';
    connectionStatus: string = 'Disconnected';

    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('http://localhost:5000/hubs/factory', {
                withCredentials: true
            })
            .withAutomaticReconnect([0, 2000, 5000, 10000])
            .configureLogging(signalR.LogLevel.Information)
            .build();
    }

    start(): void {
        if (this.isConnected) return;

        this.connection.start()
            .then(() => {
                this.isConnected = true;
                this.connectionStatus = 'Connected';
                console.log('Connected to FactoryHub');
                this.setupListeners();
            })
            .catch(err => {
                this.connectionStatus = 'Failed';
                console.error('Connection failed:', err);
                setTimeout(() => this.start(), 3000);
            });
    }

    private setupListeners(): void {
        this.connection.on('ReceiveTemperatureUpdate', (temp: number) => {
            this.temperature = temp;
            console.log('Temperature updated:', temp);
        });

        this.connection.on('ReceiveSmokeUpdate', (smoke: boolean) => {
            this.smoke = smoke;
            console.log('Smoke status:', smoke);
        });

        this.connection.on('ReceiveWeightUpdate', (weight: number) => {
            this.weight = weight;
            console.log('Weight updated:', weight);
        });

        this.connection.on('ReceiveProductNumberUpdate', (productNumber: string) => {
            this.productNumber = productNumber;
            console.log('Product number:', productNumber);
        });

        this.connection.onreconnected((connectionId) => {
            this.isConnected = true;
            this.connectionStatus = 'Reconnected';
            console.log('Reconnected to hub');
        });

        this.connection.onreconnecting((error) => {
            this.isConnected = false;
            this.connectionStatus = 'Reconnecting...';
            console.log('Attempting to reconnect:', error);
        });

        this.connection.onclose((error) => {
            this.isConnected = false;
            this.connectionStatus = 'Disconnected';
            console.log('Connection closed:', error);
        });
    }
}
