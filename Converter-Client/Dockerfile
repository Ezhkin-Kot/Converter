FROM nginx:alpine

RUN rm -rf /usr/share/nginx/html/*

COPY Pages /usr/share/nginx/html/Pages
COPY scripts /usr/share/nginx/html/scripts
COPY style /usr/share/nginx/html/style

COPY nginx.conf /etc/nginx/conf.d/default.conf

RUN chmod -R 755 /usr/share/nginx/html/Pages

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]