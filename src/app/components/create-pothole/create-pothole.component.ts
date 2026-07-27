import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { PotholeService } from '../../services/pothole.service';
import { UserService } from '../../services/user.service';
import { MessageService } from '../../services/message.service';
import { MessageListComponent } from '../message-list/message-list.component';

@Component({
  selector: 'app-create-pothole',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MessageListComponent],
  templateUrl: './create-pothole.component.html',
  styleUrls: ['./create-pothole.component.css']
})
export class CreatePotholeComponent implements OnInit {
  potholeForm!: FormGroup;
  isSubmitting = false;

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
      this.messageService.add('You must be logged in to create a pothole.', 'error');
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
