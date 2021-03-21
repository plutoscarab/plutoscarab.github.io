{% for post in site.posts %}	
* [{{ post.title }}]({{ post.url }}), {{ post.date | date: "%B %e, %Y" }}
{% endfor %}

### Miscellaneous

* [Table of generalized continued fractions with polynomial terms](/polygcf)