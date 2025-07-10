export const ButtonStyle = Object.freeze({
   Inactive: {
       'background-color': 'var(--milk-coffee-unselected)',
       'box-shadow': '-2px 2px 0 var(--dark-coffee), -3px 3px 0 var(--milk-coffee-unselected)',
       'pointer-events': 'none'
   },
   Active: {
       'background-color': 'var(--milk-coffee)',
       'box-shadow': '-2px 2px 0 var(--dark-coffee), -3px 3px 0 var(--milk-coffee)',
       'pointer-events': 'auto'
   }
});