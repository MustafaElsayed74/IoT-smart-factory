import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { SensorDashboardComponent } from './components/sensor-dashboard.component';
import { SensorService } from './services/sensor.service';

@NgModule({
    declarations: [
        SensorDashboardComponent
    ],
    imports: [
        BrowserModule,
        CommonModule
    ],
    providers: [SensorService],
    bootstrap: [SensorDashboardComponent]
})
export class AppModule { }
