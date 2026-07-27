import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { UserService } from '../../services/user.service';
import { MessageService } from '../../services/message.service';
import { MessageListComponent } from '../message-list/message-list.component';

@Component({
  selector: 'app-user-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MessageListComponent],
  templateUrl: './user-login.component.html',
  styleUrls: ['./user-login.component.css']
})
export class UserLoginComponent implements OnInit {
  loginForm!: FormGroup;
  isSubmitting = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly userService: UserService,
    private readonly messageService: MessageService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  submit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      this.messageService.clear();
      this.messageService.add('Please enter a valid email and password.', 'error');
      return;
    }

    this.isSubmitting = true;
    this.messageService.clear();

    this.userService.login(this.loginForm.value).subscribe({
      next: (response) => {
        this.userService.saveAuthResponse(response, this.loginForm.value.email);
        this.isSubmitting = false;
        this.router.navigate(['/']);
      },
      error: (error: HttpErrorResponse) => {
        this.messageService.add(this.getErrorMessage(error), 'error');
        this.isSubmitting = false;
      }
    });
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 401) {
      return 'Invalid email or password.';
    }

    return 'Unable to sign in. Please try again.';
  }
}
