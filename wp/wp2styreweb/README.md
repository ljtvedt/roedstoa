# Kode for å ekstrahere data fra WP for å kunne laste inn i Styreweb

Må handtere wpdmpro, lenker til desse, og lenker til andre downloads

    [wpdm_package, [wpdm_direct, [wpdm_category....]
    Korleis skal vi handtere kategoriar opp mot hierarkisk modell????

    Content kan innehalde lenker som er av type rel="attachment wp-att-805" og <img src class"wp-image-809"

Type for 

* Attachments
* Images
* Posts
* Page

Alle lenker bør trekkast ut som eigne felt i post og page, i  tillegg til å ligge i teksten

Finne alle attachment, og kople dei til post/page. Slå saman kategoriane for begge, for å finne kategori for filane. Lag ny path basert på kategoriar



## Post-typer
* attachment
* nav_menu_item
* custom_css
* page
* post
* wpdmpro

## Category-mapping
Lag mapping fra alle Wp-categori-typer til SW-category med prioritering. Bruk denne ved mapping til SW-modell, og bruk så desse til å berekne nye fil-stiar (basert på prioiritet)