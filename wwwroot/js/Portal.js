
function addActive() {
	var t = $(location).attr('href').split('/').pop();
	$('li>a.nav-link').each(function () {
		var e = $(this).attr('href').split('/').pop();
		$(this).removeClass('active'),
			e === t && $(this).addClass('active');
		t === "" && $('#Index').addClass('active');
	});
};

function OnBegin() {
	$("#results").empty();
}

function startLoadingAnimation() {
	let imgObj = $("#loadImg");
	imgObj.show();

	let centerY = (window.innerHeight - imgObj.height()) / 2;
	let centerX = (window.innerWidth - imgObj.width()) / 2;

	imgObj.offset({ top: centerY, left: centerX });
}

function stopLoadingAnimation() {
	$("#loadImg").hide();
}


$(document).ready(function () {
	addActive();
});

$("#results").on("click", "a.history", function (e) {
	startLoadingAnimation();
	e.preventDefault();
	$.ajax({
		cache: false,
		url: this.href,
		success: function (data) {
			stopLoadingAnimation();
			$('#dialogContent').html(data);
			$('#ModalBox').modal('show');
		},
	});
});

$("#results").on("click", "a.detail", function (e) {
	e.preventDefault();
	$.ajax({
		cache: false,
		url: this.href,
		success: function (data) {
			$('#dialogContentDetail').html(data);
			$('#ModalBoxDetail').modal('show');
		},
	});
});

$("#results").on("click", "span.detail-dropdown", function (e) {
	let self = this;
	if (this.getAttribute("type") == "closed") {
		startLoadingAnimation();
		$(this).parent().parent().after('<tr class="ajax-content"><td colspan="10"></td></tr>');
		$.ajax({
			cache: false,
			url: this.getAttribute("data"),
			type: "POST",
			data: { idReserve: this.getAttribute("idReserve") },
			success: function (data) {
				stopLoadingAnimation();
				$(self).parent().parent().next().children().append(data);
				self.setAttribute("type", "opened");
			}
		});
		let path = document.location + "/images/close.png";
		$(self).css("background-image", 'url("' + path + '")');
	} else {
		$(self).parent().parent().next().remove();
		this.setAttribute("type", "closed");
		let path = document.location + "/images/open.png";
		$(self).css("background-image", 'url("' + path + '")');
	}
});

$("#search-button").on("click", function (e) {
	let Date1 = $('#Date1').children('input').val();
	let Date2 = $('#Date2').children('input').val();
	let Text = $('#search').val();
	let token = $('#__AjaxAntiForgeryForm').children('input').prop('value');
	let data = {
		__RequestVerificationToken: token,
		Date1: Date1,
		Date2: Date2,
		Text: Text
	};
	startLoadingAnimation();
	$.ajax({
		cache: false,
		url: $('#link-for-search-button').prop('href'),
		type: "POST",
		data: $.param(data),
		success: function (data) {
			stopLoadingAnimation();
			OnBegin();
			$('#results').append(data);
		}
	});
});