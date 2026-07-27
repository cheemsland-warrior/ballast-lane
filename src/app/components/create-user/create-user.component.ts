import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { UserService } from '../../services/user.service';
import { MessageService } from '../../services/message.service';
import { MessageListComponent } from '../message-list/message-list.component';

@Component({
  selector: 'app-create-user',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MessageListComponent],
  templateUrl: './create-user.component.html',
  styleUrls: ['./create-user.component.css']
})
export class CreateUserComponent implements OnInit {
  userForm!: FormGroup;
  isSubmitting = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly userService: UserService,
    private readonly messageService: MessageService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.userForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      displayName: ['', [Validators.required, Validators.minLength(2)]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  submit(): void {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      this.messageService.clear();
      this.messageService.add('Please fix the highlighted validation errors.', 'error');
      return;
    }

    this.isSubmitting = true;
    this.messageService.clear();

    this.userService.register(this.userForm.value).subscribe({
      next: (response) => {
        this.userService.saveAuthResponse(response);
        this.messageService.add('User created successfully.', 'success');
        this.userForm.reset();
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
    const serverMessage = error.error;
    if (serverMessage) {
      return typeof serverMessage === 'string' ? serverMessage : 'Unable to create user. Please try again.';
    }

    return 'Unable to create user. Please try again.';
  }
}
