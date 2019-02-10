if (typeof (Qwerty) != 'object')
	Qwerty = {}

Qwerty.DatePicker = function (id, name, init) {
	var element = document.getElementById(id)
	element.className = 'calendar'
	element.innerHTML = '<input class="form-control mx-sm-2" size="8" onclick="Qwerty.DatePicker.Show(this.parentNode)" readonly=\"readonly\" /><div style="display:none"></div>'
	element.firstChild.name = name
	element.firstChild.value = init
}

Qwerty.DatePicker.Show = function (calendar, shift) {
	if (Qwerty.DatePicker.LastCalendar != calendar) {
		if (Qwerty.DatePicker.LastCalendar)
			Qwerty.DatePicker.LastCalendar.lastChild.style.display = 'none'
		Qwerty.DatePicker.LastCalendar = calendar
	}
	var months = ['\u042f\u043d\u0432\u0430\u0440\u044c', '\u0424\u0435\u0432\u0440\u0430\u043b\u044c', '\u041c\u0430\u0440\u0442', '\u0410\u043f\u0440\u0435\u043b\u044c', '\u041c\u0430\u0439', '\u0418\u044e\u043d\u044c', '\u0418\u044e\u043b\u044c', '\u0410\u0432\u0433\u0443\u0441\u0442', '\u0421\u0435\u043d\u0442\u044f\u0431\u0440\u044c', '\u041e\u043a\u0442\u044f\u0431\u0440\u044c', '\u041d\u043e\u044f\u0431\u0440\u044c', '\u0414\u0435\u043a\u0430\u0431\u0440\u044c']
	var d = calendar.firstChild.value
	d = shift ? new Date(calendar.getAttribute('data-year'), calendar.getAttribute('data-month') * 1 + shift, 1) : /^\d+\.\d+\.\d+$/.test(d) ? (d = d.split('.'), new Date(d[2], d[1] - 1, d[0])) : new Date()
	var month = d.getMonth()
	d.setDate(1)
	calendar.setAttribute('data-year', d.getFullYear())
	calendar.setAttribute('data-month', month)
	var s = '<table><a class="prev" href="#" onclick="return!1">&lt;</a><a class="next" href="#" onclick="return!1">&gt;</a>' + months[month] + ', ' + d.getFullYear()
	d.setDate(2 - (d.getDay() || 7))
	do {
		s += '<tr>'
		for (var i = 0; i < 7; i++)
			s += '<td>' + (d.getMonth() == month ? '<a href="#" onclick="return!1">' + d.getDate() + '</a>' : d.getDate()) + '</td>',
			d.setDate(d.getDate() + 1)
		s += '</tr>'
	} while (d.getMonth() == month)
	s += '</table>'
	calendar.lastChild.innerHTML = s
	calendar.lastChild.style.display = 'block'
	if (!calendar.onclick) {
		calendar.onclick = function (event) {
			event = event || window.event
			var e = event.target || event.srcElement
			if (e.tagName == 'A' && !e.className) {
				calendar.firstChild.value = e.firstChild.nodeValue + '.' + (calendar.getAttribute('data-month') * 1 + 1) + '.' + calendar.getAttribute('data-year')
				calendar.lastChild.style.display = 'none'
			}
			if (e.tagName == 'A' && (e.className == 'prev' || e.className == 'next'))
				Qwerty.DatePicker.Show(calendar, e.className == 'prev' ? -1 : 1)
		}
	}
}

Qwerty.DatePicker.Blur = function (event) {
	if (!Qwerty.DatePicker.LastCalendar)
		return
	event = event || window.event
	var e = event.target || event.srcElement
	if (!e.parentNode)
		return
	while (e) {
		if (e == Qwerty.DatePicker.LastCalendar)
			break
		e = e.parentNode
	}
	if (!e)
		Qwerty.DatePicker.LastCalendar.lastChild.style.display = 'none'
}

if (document.addEventListener) {
	document.addEventListener('click', Qwerty.DatePicker.Blur, false)
} else {
	document.attachEvent('onclick', Qwerty.DatePicker.Blur)
}