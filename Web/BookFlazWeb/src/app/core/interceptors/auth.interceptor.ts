import { HttpInterceptorFn, HttpRequest, HttpHandlerFn } from '@angular/common/http';

export const AuthInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {

  const tokenDev = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxMiIsImVtYWlsIjoibWlndWVsKzE3NjQyODkwMTVAZXhhbXBsZS5jb20iLCJJc0FkbWluIjoiRmFsc2UiLCJuYmYiOjE3NjQ5MDIwMzcsImV4cCI6MTc2NTUwNjgzNywiaWF0IjoxNzY0OTAyMDM3LCJpc3MiOiJCb29rRmxheklzc3VlciIsImF1ZCI6IkJvb2tGbGF6QXVkaWVuY2UifQ.e6-D3DGzMwJfafFgZ9Wgdu6kVOYXPcdrsMuaYOMhVrQ";

  if (tokenDev && req.url.includes('/api/')) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${tokenDev}`
      }
    });
  }
  return next(req);
};
