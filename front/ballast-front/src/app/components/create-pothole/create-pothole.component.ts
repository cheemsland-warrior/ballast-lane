import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { PotholeService } from '../../services/pothole.service';
import { UserService } from '../../services/user.service';
import { MessageService } from '../../services/message.service';
import { MessageListComponent } from '../message-list/message-list.component';
import { LeafletModule } from '@asymmetrik/ngx-leaflet';
import * as L from 'leaflet';

@Component({
  selector: 'app-create-pothole',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MessageListComponent, LeafletModule],
  templateUrl: './create-pothole.component.html',
  styleUrls: ['./create-pothole.component.css']
})
export class CreatePotholeComponent implements OnInit {
  potholeForm!: FormGroup;
  isSubmitting = false;

  // Leaflet Map Configuration
  mapOptions = {
    layers: [
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 18,
        attribution: '© OpenStreetMap contributors'
      })
    ],
    zoom: 13,
    // Centered on Ciudad Victoria, Tamaulipas
    center: L.latLng(23.7369, -99.1411)
  };

  // Array to hold the dynamic map marker
  mapLayers: L.Layer[] = [];

  constructor(
    private readonly fb: FormBuilder,
    private readonly potholeService: PotholeService,
    private readonly userService: UserService,
    private readonly messageService: MessageService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.potholeForm = this.fb.group({
      description: ['', [Validators.required, Validators.minLength(3)]],
      latitude: [null, [Validators.required]],
      longitude: [null, [Validators.required]],
      status: ['Reported']
    });
  }

  // Handle map clicks to place a marker and update form values
  onMapClick(event: L.LeafletMouseEvent): void {
    const lat = event.latlng.lat;
    const lng = event.latlng.lng;

    // Update the reactive form with the new coordinates
    this.potholeForm.patchValue({
      latitude: lat,
      longitude: lng
    });

    // Clear previous markers and add the new one
    // We use standard CDN URLs for the icons to prevent Angular bundling issues
    this.mapLayers = [
      L.marker([lat, lng], {
        icon: L.icon({
          iconSize: [25, 41],
          iconAnchor: [12, 41],
          iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
          shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png'
        })
      })
    ];
  }

  submit(): void {
    if (this.potholeForm.invalid) {
      this.potholeForm.markAllAsTouched();
      this.messageService.clear();
      this.messageService.add('Please complete all required pothole fields.', 'error');
      return;
    }

    const currentUser = this.userService.getCurrentUser();
    if (!currentUser) {
      this.messageService.clear();
      this.messageService.add('The user should be logged in to create a pothole.', 'error');
      return;
    }

    this.isSubmitting = true;
    this.messageService.clear();

    const payload = {
      ...this.potholeForm.value,
      userId: currentUser.id || this.userService.getCurrentUser()?.id || ''
    };

    this.potholeService.create(payload).subscribe({
      next: () => {
        this.messageService.add('Pothole created successfully.', 'success');
        this.potholeForm.reset({ status: 'Reported' });
        this.mapLayers = []; // Clear marker on success
        this.isSubmitting = false;
        this.router.navigate(['/potholes-list']);
      },
      error: (error: HttpErrorResponse) => {
        this.messageService.add(this.getErrorMessage(error), 'error');
        this.isSubmitting = false;
      }
    });
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 401) {
      return 'You are not authorized to create a pothole.';
    }
    return 'Unable to create the pothole. Please try again.';
  }
}