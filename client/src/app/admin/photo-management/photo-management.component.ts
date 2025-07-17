import { Component, inject, OnInit, signal } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { Photo } from '../../_models/photo';

@Component({
  selector: 'app-photo-management',
  imports: [],
  templateUrl: './photo-management.component.html',
  styleUrl: './photo-management.component.css'
})
export class PhotoManagementComponent implements OnInit{
  private adminService = inject(AdminService);
  photosForModeration = signal<Photo[]>([]);

  ngOnInit(): void {
    this.getPhotosForModeration();
  }

  getPhotosForModeration() {
    this.adminService.getPhotosForModeration().subscribe({
      next: data => {
        this.photosForModeration.set(data)
      }
    });
  }

  approvePhoto(photoId: number)
  {
     this.adminService.approvePhoto(photoId).subscribe({
      next: _ => {
        this.photosForModeration.update(photos => photos.filter(x => x.id !== photoId))
      }
    });
  }

  rejectPhoto(photoId: number)
  {
    this.adminService.rejectPhoto(photoId).subscribe({
      next: _ => {
        this.photosForModeration.update(photos => photos.filter(x => x.id !== photoId))
      }
    });
  }

}
