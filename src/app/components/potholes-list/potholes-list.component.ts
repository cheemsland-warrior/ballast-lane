import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PotholeService } from '../../services/pothole.service';
import { MessageService } from '../../services/message.service';
import { MessageListComponent } from '../message-list/message-list.component';
import { IPotholeDto } from '../../interfaces/pothole.interface';

@Component({
    selector: 'app-potholes-list',
    standalone: true,
    imports: [CommonModule, MessageListComponent],
    templateUrl: './potholes-list.component.html',
    styleUrls: ['./potholes-list.component.css']
})
export class PotholesListComponent implements OnInit {
    potholes: IPotholeDto[] = [];
    isLoading = signal(false);

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
            },
            error: () => {
                this.potholes = [];
                this.messageService.add('Unable to load potholes.', 'error');
            },
            complete: () => {
                this.isLoading.set(false);
            }
        });
    }
}
