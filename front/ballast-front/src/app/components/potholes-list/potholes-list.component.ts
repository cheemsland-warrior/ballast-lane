import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PotholeService } from '../../services/pothole.service';
import { MessageService } from '../../services/message.service';
import { MessageListComponent } from '../message-list/message-list.component';
import { IPotholeDto } from '../../interfaces/pothole.interface';
import { LeafletModule } from '@asymmetrik/ngx-leaflet';
import * as L from 'leaflet';

@Component({
    selector: 'app-potholes-list',
    standalone: true,
    imports: [CommonModule, RouterLink, MessageListComponent, LeafletModule],
    templateUrl: './potholes-list.component.html',
    styleUrls: ['./potholes-list.component.css']
})
export class PotholesListComponent implements OnInit {
    potholes: IPotholeDto[] = [];
    isLoading = signal(false);

    // Leaflet Map Configuration
    mapOptions = {
        layers: [
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                maxZoom: 18,
                attribution: '© OpenStreetMap contributors'
            })
        ],
        zoom: 13,
        // Centered on Ciudad Victoria
        center: L.latLng(23.7369, -99.1411)
    };

    // Array to hold the dynamic map markers
    mapLayers: L.Layer[] = [];

    constructor(
        private readonly potholeService: PotholeService,
        private readonly messageService: MessageService
    ) { }

    ngOnInit(): void {
        this.loadPotholes();
    }

    loadPotholes(): void {
        this.isLoading.set(true);
        this.potholeService.getAll().subscribe({
            next: (response) => {
                this.potholes = response;
                this.updateMapMarkers(); // Refresh markers on load
            },
            error: () => {
                this.potholes = [];
                this.updateMapMarkers();
                this.messageService.add('Unable to load potholes.', 'error');
            },
            complete: () => {
                this.isLoading.set(false);
            }
        });
    }

    deletePothole(id: string): void {
        if (!id) {
            this.messageService.add('Unable to delete pothole: invalid ID.', 'error');
            return;
        }

        this.potholeService.delete(id).subscribe({
            next: () => {
                // Remove from list and update map markers
                this.potholes = this.potholes.filter((pothole) => pothole.id !== id);
                this.updateMapMarkers(); 
                this.messageService.add('Pothole deleted successfully.', 'success');
            },
            error: () => {
                this.messageService.add('Unable to delete pothole.', 'error');
            }
        });
    }

    // Helper method to convert pothole data into map markers
   // This method converts your IPotholeDto data into map markers
private updateMapMarkers(): void {
    
    // We loop through the 'this.potholes' array
    this.mapLayers = this.potholes.map(pothole => {
        
        // We use the latitude and longitude from your IPotholeDto
        // Number() is used just in case your API returns them as strings
        const marker = L.marker([Number(pothole.latitude), Number(pothole.longitude)], {
            icon: L.icon({
                iconSize: [25, 41],
                iconAnchor: [12, 41],
                popupAnchor: [1, -34],
                iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
                shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png'
            })
        });

        // We bind the description from IPotholeDto to a popup on the marker
        marker.bindPopup(`<strong>Reported Issue:</strong><br>${pothole.description}`);
        
        return marker;
    });
}
}