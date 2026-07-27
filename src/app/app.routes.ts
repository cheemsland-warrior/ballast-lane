import { Routes } from '@angular/router';
import { CreatePotholeComponent } from './components/create-pothole/create-pothole.component';
import { CreateUserComponent } from './components/create-user/create-user.component';
import { HomeComponent } from './components/home/home.component';
import { PotholesListComponent } from './components/potholes-list/potholes-list.component';
import { UserLoginComponent } from './components/user-login/user-login.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'create-user', component: CreateUserComponent },
  { path: 'create-pothole', component: CreatePotholeComponent },
  { path: 'potholes-list', component: PotholesListComponent },
  { path: 'login', component: UserLoginComponent }
];
